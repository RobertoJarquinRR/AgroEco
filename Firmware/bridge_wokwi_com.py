import socket
import threading
import time
from pathlib import Path

import serial

LOCAL_COM = "COM7"
BAUD = 115200
WAIT_SECONDS = 90


def read_rfc2217_port(wokwi_toml: Path) -> int:
    try:
        import tomllib

        config = tomllib.loads(wokwi_toml.read_text(encoding="utf-8"))
        return int(config.get("wokwi", {}).get("rfc2217ServerPort", 4000))
    except Exception:
        return 4000


def wait_for_tcp_server(host: str, port: int, timeout_seconds: int) -> bool:
    deadline = time.time() + timeout_seconds
    while time.time() < deadline:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
            sock.settimeout(1.0)
            if sock.connect_ex((host, port)) == 0:
                return True
        time.sleep(0.5)
    return False


def pipe(src, dst, stop_event: threading.Event):
    while not stop_event.is_set():
        try:
            data = src.read(1024)
            if data:
                dst.write(data)
        except Exception as exc:
            print(f"[bridge] conexion interrumpida: {exc}")
            stop_event.set()


def main():
    project_root = Path(__file__).resolve().parent
    wokwi_toml = project_root / "wokwi.toml"
    port = read_rfc2217_port(wokwi_toml)

    print(f"[bridge] esperando Wokwi RFC2217 en localhost:{port}...")
    if not wait_for_tcp_server("localhost", port, WAIT_SECONDS):
        print("[bridge] no hay servidor RFC2217 activo.")
        print("[bridge] abre 'Wokwi: Start Simulator' y deja visible la pestana del simulador.")
        return

    wokwi_url = f"rfc2217://localhost:{port}"
    try:
        wokwi = serial.serial_for_url(wokwi_url, baudrate=BAUD, timeout=0.01)
    except Exception as exc:
        print(f"[bridge] no se pudo abrir {wokwi_url}: {exc}")
        return

    try:
        local = serial.Serial(LOCAL_COM, BAUD, timeout=0.01)
    except Exception as exc:
        print(f"[bridge] no se pudo abrir {LOCAL_COM}: {exc}")
        wokwi.close()
        return

    stop_event = threading.Event()
    threading.Thread(target=pipe, args=(wokwi, local, stop_event), daemon=True).start()
    threading.Thread(target=pipe, args=(local, wokwi, stop_event), daemon=True).start()

    print(f"[bridge] activo: {wokwi_url} <-> {LOCAL_COM}")
    print("[bridge] presiona Ctrl+C para cerrar.")

    try:
        while not stop_event.is_set():
            time.sleep(0.2)
    except KeyboardInterrupt:
        pass
    finally:
        stop_event.set()
        local.close()
        wokwi.close()


if __name__ == "__main__":
    main()