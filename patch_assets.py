import argparse
import os
import shutil
import sys
import zipfile
from pathlib import Path

# --- CONFIGURATION ---
# Paths can be absolute, or relative to where this script is saved
LIVE_UNITY_DIR = r"C:\Users\Kamil\Projects\UniMaasGit\IUI-VR-BCI-Group9\VR"
SOURCE_BACKUP_DIR = r"C:\Users\Kamil\Projects\UniMaas\IUI\Lab 3 - VR - Code.zip"  # Can be a folder OR a .zip file
ASSET_LIST_FILE = r"asset_list.txt"
LOG_FILE_STATUS = False
# ---------------------

DIM_YELLOW = "\033[2;33m"
RED = "\033[31m"
RESET = "\033[0m"


def colorize(text, color):
    if sys.stdout.isatty():
        return f"{color}{text}{RESET}"
    return text


def patch_assets(log_file_status=LOG_FILE_STATUS):
    project_base = Path(LIVE_UNITY_DIR)
    source_path = Path(SOURCE_BACKUP_DIR)
    list_path = Path(ASSET_LIST_FILE)
    
    if not project_base.exists() or not project_base.is_dir():
        print(f"❌ Error: LIVE_UNITY_DIR '{LIVE_UNITY_DIR}' is not a valid directory.")
        return
    if not source_path.exists():
        print(f"❌ Error: SOURCE_BACKUP_DIR '{SOURCE_BACKUP_DIR}' does not exist.")
        return
    if not list_path.exists():
        print(f"❌ Error: ASSET_LIST_FILE '{ASSET_LIST_FILE}' not found.")
        return

    is_zip = zipfile.is_zipfile(source_path) or source_path.suffix.lower() == '.zip'
    
    if is_zip:
        print(f"📦 Source identified as a ZIP archive. Handling single file subpath rules...")
    elif source_path.is_dir():
        print(f"📁 Source identified as a standard directory.")
    else:
        print(f"❌ Error: SOURCE_BACKUP_DIR is neither a directory nor a valid zip file.")
        return

    copied_count = 0
    missing_files = []

    with open(list_path, 'r', encoding='utf-8') as f:
        relative_paths = [line.strip().replace('\\', '/').lstrip('/') for line in f if line.strip()]

    print(f"🔍 Found {len(relative_paths)} total target files to process...")

    # --- PROCESS ZIP ARCHIVE ---
    if is_zip:
        with zipfile.ZipFile(source_path, 'r') as archive:
            zip_contents = {}
            
            for name in archive.namelist():
                # Skip directory entries themselves
                if name.endswith('/'):
                    continue
                
                normalized_name = name.replace('\\', '/').lstrip('/')
                
                # Strip the single root subpath folder if it exists
                # e.g., "MasterPack-v1.0/Assets/Textures/art.png" -> "Assets/Textures/art.png"
                parts = normalized_name.split('/')
                if len(parts) > 1:
                    stripped_name = '/'.join(parts[1:])
                else:
                    stripped_name = normalized_name
                
                # Map both the stripped path and the full path just in case
                zip_contents[stripped_name] = name
                zip_contents[normalized_name] = name

            for rel_path in relative_paths:
                dest_file = project_base / rel_path
                
                if rel_path in zip_contents:
                    internal_zip_name = zip_contents[rel_path]
                    dest_file.parent.mkdir(parents=True, exist_ok=True)
                    
                    with archive.open(internal_zip_name) as source_stream, open(dest_file, 'wb') as dest_stream:
                        shutil.copyfileobj(source_stream, dest_stream)
                    copied_count += 1
                    if log_file_status:
                        print(colorize(f"copied file: {rel_path}", DIM_YELLOW))
                else:
                    missing_files.append(rel_path)
                    if log_file_status:
                        print(colorize(f"missing file: {rel_path}", RED))

    # --- PROCESS STANDARD DIRECTORY ---
    else:
        for rel_path in relative_paths:
            src_file = source_path / rel_path
            dest_file = project_base / rel_path

            if src_file.exists() and src_file.is_file():
                dest_file.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(src_file, dest_file)
                copied_count += 1
                if log_file_status:
                    print(colorize(f"copied file: {rel_path}", DIM_YELLOW))
            else:
                missing_files.append(rel_path)
                if log_file_status:
                    print(colorize(f"missing file: {rel_path}", RED))

    # --- RESULTS REPORT ---
    print("\n" + "="*50)
    print("📊 EXTERNAL PATCHING RESULTS")
    print("="*50)
    print(f"Target Unity Project: {project_base.resolve()}")
    print(f"✅ Successfully patched: {copied_count} files")
    print(f"❌ Missing in source:    {len(missing_files)} files")
    print("="*50)
    
    if missing_files:
        missing_log_path = list_path.parent / "missing_assets.txt"
        with open(missing_log_path, "w", encoding="utf-8") as out:
            out.write("\n".join(missing_files))
        print(f"\n⚠️ {len(missing_files)} files were missing from the source target.")
        print(f"📝 Full list of missing files saved to: {missing_log_path}")
    else:
        print("\n🎉 Success! Every asset on your list was perfectly restored and uncorrupted.")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Patch Unity assets from a backup source.")
    parser.add_argument(
        "--log-files",
        action="store_true",
        help="log each copied or missing file while processing",
    )
    args = parser.parse_args()
    patch_assets(log_file_status=args.log_files)