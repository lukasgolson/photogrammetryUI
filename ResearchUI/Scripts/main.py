import os
import sys

sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from update_environment import update_env_task
from update_project import update_project_task


if __name__ == "__main__":
    # Check and update the repository
    update_env_task()
    update_project_task()
