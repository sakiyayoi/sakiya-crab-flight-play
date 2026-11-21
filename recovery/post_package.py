"""Finish the explicitly partial R1 WebGL package; does not change the Player."""
from pathlib import Path
import shutil

source = Path(__file__).resolve().parents[1]
output = source.parent / "web-r1"
version = (source / "VERSION").read_text().strip()
loaders = list((output / "Build").glob("*.loader.js"))
if len(loaders) != 1:
    raise SystemExit("Expected exactly one built Unity loader")
built_name = loaders[0].name.removesuffix(".loader.js")
host = (source / "games/flight/web-template/host.js").read_text()
(output / "host.js").write_text(host.replace("Build/" + version + ".", "Build/" + built_name + "."))
shutil.copytree(source / "recovery/runtime-assets/SoundPack", output / "SoundPack", dirs_exist_ok=True)
shutil.copytree(source / "voice", output / "voice", dirs_exist_ok=True)
shutil.copyfile(source / "CREDITS.md", output / "CREDITS.md")
print("Packaged", version, "using built asset prefix", built_name)
