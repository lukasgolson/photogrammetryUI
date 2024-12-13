import subprocess
import sys
from datetime import date

last_update_file = "last_update.txt"


def upgrade_pip():
    try:
        print("Upgrading pip...")
        print("Current Working Directory: ", sys.path[0])
        subprocess.run([sys.executable, "Python/pip.pyz", "install", "--upgrade", "pip"], check=True)
        print("pip upgraded successfully.")
    except subprocess.CalledProcessError as e:
        print(f"Failed to upgrade pip: {e}")
        sys.exit(1)  # Exit if pip upgrade fails


def update_all_packages():
    try:
        # Get the list of outdated packages
        print("Checking for outdated packages...")
        result = subprocess.run(
            [sys.executable, "-m", "pip", "list", "--outdated"],
            stdout=subprocess.PIPE,
            text=True,
            check=True,
        )
        outdated_packages = result.stdout.strip().split("\n")

        if not outdated_packages or outdated_packages == ['']:
            print("All packages are up to date.")
            return

        # Parse outdated packages and update them
        print("Updating the following packages:")
        for package in outdated_packages:
            pkg_name = package.split("==")[0]
            print(f" - {pkg_name}")
            subprocess.run([sys.executable, "-m", "pip", "install", "--upgrade", pkg_name], check=True)

        print("All packages have been updated.")
    except subprocess.CalledProcessError as e:
        print(f"An error occurred: {e}")
        sys.exit(1)


def read_last_update(file_path):
    """Reads the last update date from the file. Returns None if the file does not exist."""
    try:
        with open(file_path, "r") as f:
            return f.read().strip()
    except FileNotFoundError:
        return None
    except Exception as e:
        print(f"Error reading the file {file_path}: {e}")
        return None


def write_last_update(file_path):
    """Writes today's date to the file to record the last update."""
    try:
        today = str(date.today())

        with open(file_path, "w") as f:
            f.write(str(today))
    except Exception as e:
        print(f"Error writing to the file {file_path}: {e}")


def should_update():
    # Check if the last update file exists and read its content
    last_update = read_last_update(last_update_file)

    if last_update is None:
        # If no previous record exists, initialize it and perform update
        write_last_update(last_update_file)

        return True

    if last_update == str(date.today()):
        # If the update was already done today, no need to update again
        return False

    return True


if __name__ == "__main__":
    if should_update():
        print("Updating packages")
        # Step 1: Upgrade pip
        upgrade_pip()

        # Step 2: Update all packages
        update_all_packages()

        write_last_update(last_update_file)

    else:
        print("Packages are up to date.")
