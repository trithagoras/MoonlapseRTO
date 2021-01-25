import math

from twisted.internet.protocol import connectionDone
from twisted.protocols.basic import NetstringReceiver

import datetime

from typing import *

from . import packet
from .entities import models


def debug(message: str):
    print(message)


class MoonlapseProtocol(NetstringReceiver):
    def __init__(self, server):
        self.server = server
        self.state = self.ENTRY
        self.queued_packets = []    # packets to send to client on next tick

        self.datetime = datetime.datetime.now()

        self.player: Optional[models.Instance] = None
        self.visible_instances: Set[models.Instance] = set()

        self.logged_in = False

    def connectionMade(self):
        self.server.connected_protocols.add(self)

    def connectionLost(self, reason=connectionDone):
        debug("Lost connection from a client.")

        if self.logged_in:
            self.logout()
        self.server.connected_protocols.remove(self)

    def stringReceived(self, string):
        p = packet.frombytes(string)
        debug(f"Received packet from my client {p}")

        self.process_packet(p)

    def send_packet(self, p: packet.Packet):
        """
        Sends a packet to this protocol's client.
        Call this to communicate information back to the game client application.
        """
        self.sendString(p.tobytes())
        debug(f"Sent data to my client: {p.tobytes()}")

    def process_packet(self, p: packet.Packet, from_client=True):
        self.state(p, from_client)

    def ENTRY(self, p: packet.Packet, from_client=True):
        if from_client:
            if p.a == 'Login':
                self.login(p)
            elif p.a == 'Register':
                self.register(p)
                pass

    def login(self, p: packet.Packet):
        username = p.p[0]
        password = p.p[1]

        if not models.DbUser.objects.filter(username=username):
            self.queued_packets.append(packet.construct_deny_packet("I don't know anybody by that name"))
            return

        user = models.DbUser.objects.get(username=username)

        self.send_packet(packet.construct_ok_packet())

        self.logged_in = True

        # construct instance and send id to player
        self.player = models.Instance(user.dbinstance_id)
        self.queued_packets.append(packet.construct_id_packet(self.player.pk))

        self.state = self.PLAY

        # hello
        self.broadcast(packet.construct_hello_packet(self.player.pk))

        # details
        self.broadcast(
            packet.construct_details_packet(self.player.pk, self.player.entity.get_component(models.Details).name), include_self=True)

        # initial position
        self.broadcast(packet.construct_position_packet(self.player.pk, self.player.x, self.player.y), include_self=True)

        # getting greeted by other players (todo: only in view)
        for proto in self.server.connected_protocols:
            other = proto.player
            if other and other != self.player:
                self.process_packet(packet.construct_hello_packet(other.pk))
                self.process_packet(
                    packet.construct_details_packet(other.pk, other.entity.get_component(models.Details).name), from_client=False)
                self.process_packet(packet.construct_position_packet(other.pk, other.x, other.y), from_client=False)

        # server time
        self.process_packet(packet.construct_time_packet(self.server.minute), from_client=False)

    def register(self, p: packet.Packet):
        username = p.p[0]
        password = p.p[1]

        if models.DbUser.objects.filter(username=username):
            self.queued_packets.append(packet.construct_deny_packet("Somebody else already goes by that name"))
            return

        # create entity
        entity = models.construct_player_dbentity(username)
        dbi = models.DbInstance.objects.get(dbentity=entity)

        # Save the new user
        user = models.DbUser(username=username, password=password, dbinstance=dbi)
        user.save()

        self.send_packet(packet.construct_ok_packet())

    def PLAY(self, p: packet.Packet, from_client=True):
        if from_client:
            if p.a == 'Move':
                self.move(p)
            elif p.a == 'Logout':
                self.logout()
            elif p.a == 'Chat':
                self.chat(p)
        else:
            if p.a == 'Move':
                self.queued_packets.append(p)
            elif p.a == 'Position':
                self.queued_packets.append(p)
            elif p.a == 'Goodbye':
                self.queued_packets.append(p)
            elif p.a == 'Log':
                self.queued_packets.append(p)
            elif p.a == 'Chat':
                self.queued_packets.append(p)
            elif p.a == 'Details':
                self.queued_packets.append(p)
            elif p.a == 'Hello':
                self.queued_packets.append(p)
            elif p.a == "Time":
                self.queued_packets.append(p)

    def logout(self):
        self.state = self.ENTRY
        self.broadcast(packet.construct_goodbye_packet(self.player.pk))
        self.player.save()
        self.player = None
        self.visible_instances = set()
        self.logged_in = False

    def chat(self, p: packet.Packet):
        """
        Broadcasts a chat message which includes this protocol's connected player name.
        Truncates to 60 characters. Cannot be empty.
        """
        message: str = p.p[1]
        if message.strip() != '':
            self.broadcast(packet.construct_chat_packet(self.player.entity.get_component(models.Details).name, p.p[0], message[:80]), include_self=True)

    def move(self, p: packet.Packet):
        x = p.p[0]
        y = p.p[1]

        # prevent cheating (sending packet with higher velocity)
        if x > 0:
            x = 1
        elif x < 0:
            x = -1

        if y > 0:
            y = 1
        elif y < 0:
            y = -1

        # normalize vector
        mag = math.sqrt(x**2 + y**2)
        if mag != 0:
            x /= mag
            y /= mag

        self.player.dx = x
        self.player.dy = y

        self.broadcast(packet.construct_move_packet(self.player.pk, self.player.dx, self.player.dy))
        self.broadcast(packet.construct_position_packet(self.player.pk, self.player.x, self.player.y), include_self=True)

    def tick(self):
        """
        Every server tick, each proto's individual tick method is called.
        """

        # delta time
        time = self.datetime
        self.datetime = datetime.datetime.now()

        delta = self.datetime - time
        deltatime = delta.total_seconds()

        if self.player:
            self.player.x += self.player.dx * deltatime * 64
            self.player.y += self.player.dy * deltatime * 64

        # send all packets in queue back to client in order
        for p in list(self.queued_packets):
            self.send_packet(p)
            self.queued_packets.remove(p)

    def broadcast(self, p: packet.Packet, include_self=False):
        protos = set(self.server.connected_protocols)
        if not include_self:
            protos.remove(self)

        for proto in protos:
            proto.process_packet(p, from_client=False)
