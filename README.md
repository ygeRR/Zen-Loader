ZenLoader

ZenLoader is a security and automation utility designed to protect systems from specific malicious domains. It operates by intercepting requests at the operating system level using the hosts file, effectively preventing applications (such as ZenAutobot) from reaching potentially harmful remote servers.

🚀 Key Features

    Automated Domain Blocking: Automatically appends known malicious domains and IP addresses to the system hosts file, redirecting them to 0.0.0.0.

    Safety First (Backup & Restore):

        Automatically creates a backup of your original hosts file (hosts.ygeR) before making any changes.

        Restores the original hosts file automatically when the application is closed.

    Version Verification: Uses MD5 hash validation to ensure compatibility with specific supported executable versions (v5.2, v5.3, and v5.8).

    Persistence & Logging: Maintains a rolling log buffer (up to 2000 lines) and saves session logs to the local AppData folder for troubleshooting.
    

🛡️ Security Logic

The application targets specific addresses to prevent the background download of unauthorized files (e.g., AutoitX3.exe). The following addresses are redirected to a non-routable address (0.0.0.0):

    george-82.webselfsite.net

    52.210.211.82

    tenahuzemeno.blogspot.com

    gargoilerdisar.blogspot.com

    resources.blogblog.com

    jimmy-03.webselfsite.net 

🛠️ Requirements

    Permissions: Must be Run as Administrator. Modifying the hosts file located in C:\Windows\System32\drivers\etc requires elevated system privileges.

    Framework: Built on .NET Framework 4.5.

📂 Installation & Usage

    Place the ZenLoader executable in the same directory as your application.

    Launch ZenLoader as an Administrator.

    The tool will automatically:

        Verify the hash of existing .exe files in the directory.

        Back up your current hosts file.

        Inject the blocklist.

    Once finished, closing the application will revert all changes to your system's hosts file.

⚠️ Disclaimer

This tool modifies system files. While it includes automated backup and restore functionality, users should ensure they have sufficient rights and understand that it is provided "as-is" for security purposes.
