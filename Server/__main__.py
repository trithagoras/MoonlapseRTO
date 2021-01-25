from twisted.internet import reactor

from server import manage
from server.mlserver import MoonlapseServer


if __name__ == '__main__':
    print(f"Starting LoC server")
    PORT: int = 42523
    reactor.listenTCP(PORT, MoonlapseServer())
    print(f"Server listening on port {42523}")
    reactor.run()
