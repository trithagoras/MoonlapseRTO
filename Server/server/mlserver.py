from twisted.internet.protocol import Factory
from twisted.internet import task
from typing import *

from . import protocol
from . import packet


class MoonlapseServer(Factory):
    def __init__(self):
        # all protocols connected to server
        self.connected_protocols: Set[protocol.MoonlapseProtocol] = set()

        self.tickrate = 60      # hertz (ticks per second)

        tick_loop = task.LoopingCall(self.tick)
        tick_loop.start(1/self.tickrate, False)

        # save all instances to DB after loop
        dbsaveloop = task.LoopingCall(self.dbsave)
        dbsaveloop.start(20, False)  # todo: 20s for testing; obvs should be less frequent

        self.minute = 0     # current minute of the day. mod 1440 (60*24)
        minuteloop = task.LoopingCall(self.progress_minute)
        minuteloop.start(2, False)

    def buildProtocol(self, addr):
        print("Adding a new client.")
        return protocol.MoonlapseProtocol(self)

    def tick(self):
        """
        Where all updates happen. Tick rate is how many updates per second.
        """
        for proto in self.connected_protocols:
            proto.tick()

    def dbsave(self):
        for proto in self.connected_protocols:
            if proto.player:
                proto.process_packet(packet.construct_log_packet("Game has been saved."), from_client=False)
                proto.player.save()
        print("Saved to DB.")

    def progress_minute(self):
        self.minute += 1
        self.minute %= 1440
