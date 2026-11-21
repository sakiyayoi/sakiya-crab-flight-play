from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import argparse

class CompactHandler(SimpleHTTPRequestHandler):
    def guess_type(self, path):
        return super().guess_type(path[:-3] if path.endswith('.br') else path)

    def end_headers(self):
        if self.path.split('?', 1)[0].endswith('.br'):
            self.send_header('Content-Encoding', 'br')
        self.send_header('Cache-Control', 'no-store')
        super().end_headers()

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Private local CalmR4 WebGL player')
    parser.add_argument('--port', type=int, default=8765)
    args = parser.parse_args()
    root = Path(__file__).resolve().parent
    print(f'CalmR4 local player: http://127.0.0.1:{args.port}/', flush=True)
    ThreadingHTTPServer(('127.0.0.1', args.port), partial(CompactHandler, directory=str(root))).serve_forever()
