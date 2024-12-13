from datetime import date

import environment_helpers as eh

last_update_file = "last_update.txt"


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


def update_env_task():
    if should_update():
        print("Updating packages")
        # Step 1: Upgrade pip
        eh.upgrade_pip()

        # Step 2: Update all packages
        eh.update_all_packages()

        write_last_update(last_update_file)

    else:
        print("Packages are up to date.")


   
