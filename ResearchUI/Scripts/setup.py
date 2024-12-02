import subprocess
import sys

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

if __name__ == "__main__":
    # Step 1: Upgrade pip
    upgrade_pip()

    # Step 2: Update all packages
    update_all_packages()
