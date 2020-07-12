# CopyDiff

Simple C# console program to copy files from one directory (and subdirs) to another, but only if the files are different or missing.

Compares using file size, then using MD5 hash if the sizes match.

Ignores files and directories starting with ".". Other options may follow.
