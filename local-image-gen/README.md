local-image-gen
===============

A local image generation model.

## Install.

Requirements:

 - [pipenv](https://pipenv.pypa.io/en/latest/): to install/manage python environments and the python version.

Then run the following:

```sh
pipenv install
pipenv shell
```

## Usage.

This will download the model's weights from HuggingFace. Roughly ~10GB.

```sh
python cache_models.py
```

This will generate a sample image. Roughly ~30s on Mac M1.

```sh
python generate.py
```

