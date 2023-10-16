import torch
from diffusers import StableDiffusionPipeline
import numpy as np
import requests
import torch
from PIL import Image
from io import BytesIO
from diffusers import StableDiffusionImg2ImgPipeline


# Model.
model_id_or_path = "runwayml/stable-diffusion-v1-5"
# model_id_or_path = "stabilityai/stable-diffusion-xl-base-1.0"
print("Caching model: ", model_id_or_path)

print("Downloading...")

# Pipeline.
pipe = StableDiffusionImg2ImgPipeline.from_pretrained(model_id_or_path, torch_dtype=torch.float16)

print("Done downloading.")