Sharedrive instructions
=======

## Layout.




## Setup.

### the config

 - create an S3 bucket
 - create an IAM user with the permissions to use the S3 bucket
 - fill in wiht `ACCESS_KEY_ID` and `SECRET_ACCESS_KEY`

```
[aws]
type = s3
provider = AWS
access_key_id = ACCESS_KEY_ID
secret_access_key = SECRET_ACCESS_KEY
region = ap-southeast-2
location_constraint = ap-southeast-2
acl = private
server_side_encryption = AES256
storage_class = STANDARD
```

## macOS / Unix

1. Install rclone.

2. Mount drive.
```sh
rclone serve nfs aws:/codebullet --addr 0.0.0.0:52000 --vfs-cache-max-size 1G --vfs-cache-mode=full --dir-cache-time=1s --poll-interval=3s 

mount -oport=52000,mountport=52000 0.0.0.0: ./mountpoint
```

## Windows

 1. Install WinFSP https://winfsp.dev/ 
 
 2. Download rclone.exe https://rclone.org/downloads/ 
 
 3. Configure the remote

    Open C:\Users\CodeBullet\rclone.conf
    
    Paste the content of config file above into the editor and save
    
    To test, run rclone config edit
 
 4. Mount the sharedrive.

    ```s
    # cd into your downloads directory
    # copy the path and paste below:
    rclone mount aws:/codebullet C:\PATH_TO_DOWNLOADS\rickandmortai-cloud-data --vfs-cache-max-size 1G --vfs-cache-mode=full --dir-cache-time=1s --poll-interval=3s 
    ```
