mergeInto(LibraryManager.library, {
    PhotoCountdownSyncFileSystem: function () {
        FS.syncfs(false, function (error) {
            if (error) {
                console.error("Photo Countdown file system sync failed:", error);
            }
        });
    }
});