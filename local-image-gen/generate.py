import torch
from diffusers import StableDiffusionPipeline
import numpy as np
import requests
import torch
from PIL import Image
from io import BytesIO
from diffusers import StableDiffusionImg2ImgPipeline

print("Loading model")

# Model.
# model_id_or_path = "runwayml/stable-diffusion-v1-5"
model_id_or_path = "stabilityai/stable-diffusion-xl-base-1.0"

# Pipeline.
pipe = StableDiffusionImg2ImgPipeline.from_pretrained(model_id_or_path, torch_dtype=torch.float16)

# Device.
device = "mps" # macOS
device = "cuda" # NVIDIA
pipe = pipe.to(device)

# Optimizations.
pipe.enable_attention_slicing()

# Image and prompt input.
print("Loading source image")
init_image = Image.open("source_image.png").convert("RGB")
init_image = init_image.resize((768, 512))
nameOfCharacter = "batman"
prompt = "2 close up images of " + nameOfCharacter + "'s face, 1 profile image and 1 looking front on, plain background"

# Generation.
print("prompt: ", prompt)
print('generating...')
image = pipe(prompt=prompt, image=init_image, strength=0.92, guidance_scale=7.5).images[0]

print("Finished")
image.save("gen.png")