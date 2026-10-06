"""Local two-instance UDP impairment proxy; no privileged OS/network changes.
Usage: python3 Docs/Verification/LatencyProxy.py --delay-ms 100 --loss .01
One client, loopback ports 7778 -> 7777; delay applies in each direction.
"""
import argparse
import heapq
import random
import select
import socket
import time

parser = argparse.ArgumentParser()
parser.add_argument("--delay-ms", type=float, default=100)
parser.add_argument("--loss", type=float, default=.01)
args = parser.parse_args()
random.seed(42)
front = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
front.bind(("127.0.0.1", 7778))
back = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
back.bind(("127.0.0.1", 0))
client = None
queue = []
serial = 0
print("Loopback proxy ready: delay per direction", args.delay_ms, "ms; loss", args.loss, flush=True)
try:
    while True:
        readable, _, _ = select.select([front, back], [], [], .005)
        for source in readable:
            data, peer = source.recvfrom(65535)
            if source is front:
                client = peer
                target, destination = back, ("127.0.0.1", 7777)
            else:
                if client is None:
                    continue
                target, destination = front, client
            if random.random() >= args.loss:
                serial += 1
                heapq.heappush(queue, (time.monotonic() + args.delay_ms / 1000, serial, target, destination, data))
        while queue and queue[0][0] <= time.monotonic():
            _, _, target, destination, data = heapq.heappop(queue)
            target.sendto(data, destination)
except KeyboardInterrupt:
    pass
finally:
    front.close()
    back.close()
