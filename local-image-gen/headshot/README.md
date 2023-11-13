Headshot generator
==================

Generates 3D headshot models from character names.

## Install.

Requirements:

 - [pipenv](https://pipenv.pypa.io/en/latest/): to install/manage python environments and the python version.

Then run the following:

```sh
pipenv install
pipenv shell
```


## Configure.

### Windows

Chuck your Replicate API token inside `config.py`.

### Linux, macOS

Set `REPLICATE_API_TOKEN` environment variable.


## Run.

```sh
cd headshot/
python3 server.py
```