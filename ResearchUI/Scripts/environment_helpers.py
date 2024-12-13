import subprocess
import sys

import importlib


def upgrade_pip():
    try:
        print("Upgrading pip...")
        print("Current Working Directory: ", sys.path[0])

        # Check if pip is installed
        pip_check = subprocess.run(
            [sys.executable, "-m", "pip", "--version"],
            stdout=subprocess.PIPE, stderr=subprocess.PIPE
        )

        if pip_check.returncode != 0:
            print("pip has not been installed. Using embedded pip.pyz to upgrade pip..")
            result = subprocess.run(
                [sys.executable, "Python/pip.pyz", "install", "--upgrade", "pip"],
                check=True
            )
            print(f"pip upgraded successfully using pip.pyz: {result.stdout}")
            return

        # If pip is functional, use it to upgrade itself
        result = subprocess.run(
            [sys.executable, "-m", "pip", "install", "--upgrade", "pip"],
            check=True
        )
        print(f"pip upgraded successfully: {result.stdout}")
    except subprocess.CalledProcessError as e:
        print(f"Failed to upgrade pip: {e}")
        sys.exit(1)  # Exit if pip upgrade fails
    except FileNotFoundError:
        print("The required pip.pyz file or executable is missing.")
        sys.exit(1)


def install_package(package_name):
    if is_package_installed(package_name):
        return
    try:
        print(f"Installing {package_name}...")
        result = subprocess.run(
            [sys.executable, "-m", "pip", "install", package_name],
            check=True
        )
        print(f"{package_name} installed successfully: {result.stdout}")
    except subprocess.CalledProcessError as e:
        print(f"Failed to install {package_name}: {e}")
        sys.exit(1)  # Exit if package installation fails


def is_package_installed(package_name):
    try:
        importlib.import_module(package_name)
        return True
    except ImportError:
        return False


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
