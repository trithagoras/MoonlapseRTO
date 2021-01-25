import json


class Packet:
    def __init__(self, a="", p=()):
        self.a = a
        if isinstance(p, str):
            self.p = []
            self.p.append(p)
        else:
            self.p = list(p)

    def tobytes(self):
        serialize_dict = {'a': self.a, 'p': self.p}

        data = json.dumps(serialize_dict, separators=(',', ':')).encode('utf-8')
        return data

    def __repr__(self) -> str:
        return f"{self.a}: {self.p}"


def frombytes(data: bytes) -> Packet:
    obj_dict = json.loads(data)
    p = Packet(obj_dict['a'], obj_dict['p'])
    return p


def construct_ok_packet():
    return Packet("Ok")


def construct_id_packet(ipk):
    return Packet("Id", [ipk])


def construct_deny_packet(reason=""):
    return Packet("Deny", reason)


def construct_position_packet(ipk, x, y):
    return Packet("Position", (ipk, round(float(x), 2), round(float(y), 2)))


def construct_move_packet(ipk, dx, dy):
    return Packet("Move", (ipk, round(float(dx), 2), round(float(dy), 2)))


def construct_goodbye_packet(ipk):
    return Packet("Goodbye", [ipk])


def construct_log_packet(message: str):
    return Packet("Log", message)


def construct_chat_packet(userfrom: str, context: str, message):
    return Packet("Chat", (userfrom, context, message))


def construct_hello_packet(ipk):
    return Packet("Hello", [ipk])


def construct_details_packet(ipk, name: str):
    return Packet("Details", (ipk, name))


def construct_time_packet(time):
    return Packet("Time", [time])
