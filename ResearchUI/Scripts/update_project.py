import os
import environment_helpers as eh

eh.install_package("pygit2")
eh.install_package("keyring")
eh.install_package("pyFLTK")

import pygit2
import keyring
from fltk import *


def show_message(title, message):
    """Utility function to display a message box with a native-like appearance."""
    msg_window = Fl_Window(300, 150, title)
    msg_window.color(FL_WHITE)  # Set background color to white
    msg_window.begin()  # Start group for widget layout

    msg_box = Fl_Box(10, 10, 280, 80, message)
    msg_box.labelsize(14)
    msg_box.align(FL_ALIGN_CENTER | FL_ALIGN_INSIDE)

    ok_button = Fl_Button(100, 100, 100, 30, "OK")
    ok_button.labelsize(12)

    def close_window(widget):
        msg_window.hide()

    ok_button.callback(close_window)

    msg_window.end()  # End group for widget layout
    msg_window.set_modal()  # Make the window modal
    msg_window.show()


def get_github_token(force_new=False):
    """
    Retrieve the GitHub token from the keyring or prompt the user to enter it.
    If force_new is True, always prompt for a new token.
    """
    service_name = "GitHub"

    if not force_new:
        token = keyring.get_password(service_name, "default")
        if token:
            return token

    # Create a dialog for input
    input_window = Fl_Window(300, 200, "GitHub Token")
    input_window.color(FL_WHITE)  # Set background color to white
    input_window.begin()  # Start group for widget layout

    input_label = Fl_Box(10, 10, 280, 20, "Enter your GitHub token:")
    input_label.labelsize(12)

    input_box = Fl_Input(80, 40, 200, 30)
    input_box.labelsize(12)
    input_box.when(FL_WHEN_ENTER_KEY | FL_WHEN_NOT_CHANGED)  # Handle input changes

    ok_button = Fl_Button(100, 100, 100, 30, "OK")
    ok_button.labelsize(12)

    def on_ok_button_click(widget):
        # Save the token when the button is clicked
        token = input_box.value()
        if token:
            keyring.set_password(service_name, "default", token)
            show_message("Token Saved", "Your GitHub token has been securely saved.")
            input_window.hide()  # Close the input window
        else:
            show_message("Authentication Failed", "A GitHub token is required to continue.")

    ok_button.callback(on_ok_button_click)

    input_window.end()  # End group for widget layout
    input_window.set_modal()  # Make the window modal
    input_window.show()

    # Start the FLTK main loop
    Fl.run()

    return input_box.value()  # Return the token entered


def create_callbacks(private, force_new_token=False):
    """
    Create remote callbacks for authentication if the repository is private.
    If force_new_token is True, prompt for a new token.
    """
    if private:
        token = get_github_token(force_new=force_new_token)
        if token:  # Check if the token is not empty
            return pygit2.RemoteCallbacks(credentials=pygit2.UserPass("x-access-token", token))
    return None  # No authentication needed for public repositories


def check_and_update_repo(remote_url, project_dir, private=False):
    def try_operation(operation):
        """
        Wrapper for repository operations to handle token expiry.
        If an operation fails, prompt for a new token and retry once.
        """
        try:
            operation()
        except pygit2.GitError as e:
            if "authentication" in str(e).lower():
                print("Authentication failed. Requesting a new token...")
                callbacks = create_callbacks(private, force_new_token=True)
                operation(callbacks)
            else:
                raise e

    # Clone if the repository does not exist
    if not os.path.exists(project_dir):
        print("Cloning the repository into the specified folder...")
        try_operation(lambda callbacks=create_callbacks(private): pygit2.clone_repository(
            remote_url, project_dir, callbacks=callbacks))
        print(f"Repository cloned into {project_dir}.")
        return

    # Open the existing repository
    try:
        repo = pygit2.Repository(project_dir)
    except pygit2.GitError:
        print("Error: The directory is not a valid Git repository.")
        return

    # Fetch updates from the remote
    remote = repo.remotes["origin"]

    def fetch_updates(callbacks):
        print("Fetching updates from remote...")
        remote.fetch(callbacks=callbacks)

    try_operation(lambda callbacks=create_callbacks(private): fetch_updates(callbacks))

    # The rest of your logic continues here...


def create_callbacks(private, force_new_token=False):
    """
    Create remote callbacks for authentication if the repository is private.
    If force_new_token is True, prompt for a new token.
    """
    if private:
        token = get_github_token(force_new=force_new_token)
        return pygit2.RemoteCallbacks(credentials=pygit2.UserPass("x-access-token", token))
    return None  # No authentication needed for public repositories


def check_and_update_repo(remote_url, project_dir, branch_name="master", private=False):
    def try_operation(operation):
        """
        Wrapper for repository operations to handle token expiry.
        If an operation fails, prompt for a new token and retry once.
        """
        try:
            operation()
        except pygit2.GitError as e:
            if "authentication" in str(e).lower():
                print("Authentication failed. Requesting a new token...")
                callbacks = create_callbacks(private, force_new_token=True)
                operation(callbacks)
            else:
                raise e

    # Clone if the repository does not exist
    if not os.path.exists(project_dir):
        print("Cloning the repository into the specified folder...")
        try_operation(lambda callbacks=create_callbacks(private): pygit2.clone_repository(
            remote_url, project_dir, callbacks=callbacks))
        print(f"Repository cloned into {project_dir}.")
        return

    # Open the existing repository
    try:
        repo = pygit2.Repository(project_dir)
    except pygit2.GitError:
        print("Error: The directory is not a valid Git repository.")
        return

    # Fetch updates from the remote
    remote = repo.remotes["origin"]

    def fetch_updates(callbacks):
        print("Fetching updates from remote...")
        remote.fetch(callbacks=callbacks)

    try_operation(lambda callbacks=create_callbacks(private): fetch_updates(callbacks))

    local_branch = repo.lookup_reference(f"refs/heads/{branch_name}")
    remote_branch = repo.lookup_reference(f"refs/remotes/origin/{branch_name}")

    # Compare the commits
    local_commit = repo[local_branch.target]
    remote_commit = repo[remote_branch.target]

    if local_commit.id == remote_commit.id:
        print("The repository is up-to-date.")
        return

    print("The remote repository has updates.")
    # Notify user and ask for confirmation
    if prompt_user_for_update():
        # Fast-forward the local branch
        print("Updating the repository...")
        repo.checkout(f"refs/heads/{branch_name}", strategy=pygit2.GIT_CHECKOUT_SAFE)
        repo.references[f"refs/heads/{branch_name}"].set_target(remote_commit.id)
        print("Repository updated.")
    else:
        print("Update declined by user.")


def prompt_user_for_update():
    """Prompt the user for confirmation to update using a message box."""
    msg_window = Fl_Window(300, 150, "Update Available")
    msg_window.color(FL_WHITE)  # Set background color to white
    msg_window.begin()  # Start group for widget layout

    msg_box = Fl_Box(10, 10, 280, 60, "The remote repository has updates. Do you want to install them?")
    msg_box.labelsize(12)
    msg_box.align(FL_ALIGN_CENTER | FL_ALIGN_INSIDE)

    # Create a variable to hold the user's choice
    user_choice = [None]  # Use a list to allow modification inside the callback

    def on_yes_click(widget):
        user_choice[0] = True  # Update choice to True
        msg_window.hide()  # Close the window

    def on_no_click(widget):
        user_choice[0] = False  # Update choice to False
        msg_window.hide()  # Close the window

    yes_button = Fl_Button(75, 100, 60, 30, "Yes")
    no_button = Fl_Button(175, 100, 60, 30, "No")

    yes_button.callback(on_yes_click)
    no_button.callback(on_no_click)

    msg_window.end()  # End group for widget layout
    msg_window.set_modal()  # Make the window modal
    msg_window.show()  # Show the dialog window

    # Run the FLTK event loop until the window is hidden
    while user_choice[0] is None:
        Fl.check()

    return user_choice[0]  # Return the user's choice



def update_project_task():
    # Define the remote URL and the specific folder where to clone
    remote_url = "https://github.com/lukasgolson/PhotogrammetryPipeline.git"
    project_dir = "Project"  # Change this path as needed
    check_and_update_repo(remote_url, project_dir, private=True)


# If you want to run the update task directly
if __name__ == "__main__":
    update_project_task()
