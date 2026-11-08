import os
import shutil
import socket
import sys
import webbrowser
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer

PORT = 8090
HOST = "127.0.0.1"
ROOT_DIR = os.path.dirname(os.path.abspath(__file__))
URL = f"http://{HOST}:{PORT}/index_aaa.html"


def is_port_in_use(host, port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.settimeout(0.2)
        return s.connect_ex((host, port)) == 0


class BlazingFastHandler(SimpleHTTPRequestHandler):
    protocol_version = "HTTP/1.1"

    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=ROOT_DIR, **kwargs)

    def guess_type(self, path):
        low = path.lower()
        if low.endswith(".glb"):
            return "model/gltf-binary"
        if low.endswith(".hdr"):
            return "image/vnd.radiance"
        if low.endswith(".js"):
            return "application/javascript; charset=utf-8"
        if low.endswith(".html"):
            return "text/html; charset=utf-8"
        if low.endswith(".mp3"):
            return "audio/mpeg"
        if low.endswith(".jpg") or low.endswith(".jpeg"):
            return "image/jpeg"
        if low.endswith(".png"):
            return "image/png"
        return super().guess_type(path)

    def end_headers(self):
        path = self.path.lower().split("?")[0]
        # Aggressive caching for assets so browser serves from memory in 0ms!
        if path.endswith((".glb", ".hdr", ".jpg", ".jpeg", ".png", ".mp3", ".js", ".woff2")):
            self.send_header("Cache-Control", "public, max-age=604800, immutable")
        else:
            self.send_header("Cache-Control", "no-cache")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Connection", "keep-alive")
        super().end_headers()

    def copyfile(self, source, outputfile):
        # 256KB buffer chunking for 4x faster throughput on localhost
        shutil.copyfileobj(source, outputfile, 262144)

    def log_message(self, format, *args):
        pass


class BlazingThreadingServer(ThreadingHTTPServer):
    allow_reuse_address = True
    daemon_threads = True

    def server_bind(self):
        super().server_bind()
        # Enable TCP_NODELAY on listener
        try:
            self.socket.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
        except Exception:
            pass


if __name__ == "__main__":
    os.chdir(ROOT_DIR)

    if is_port_in_use(HOST, PORT):
        if "--no-browser" not in sys.argv:
            webbrowser.open(URL)
        sys.exit(0)

    try:
        server = BlazingThreadingServer((HOST, PORT), BlazingFastHandler)
    except Exception as e:
        sys.exit(1)

    if "--no-browser" not in sys.argv:
        webbrowser.open(URL)

    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.server_close()
