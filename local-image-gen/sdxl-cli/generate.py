import torch
from diffusers import StableDiffusionPipeline
import numpy as np
import requests
import torch
from PIL import Image
from io import BytesIO
from diffusers import StableDiffusionImg2ImgPipeline, StableDiffusionXLImg2ImgPipeline
from diffusers.utils import load_image

print("Loading model")

# Model.
# model_id_or_path = "runwayml/stable-diffusion-v1-5"
# model_id_or_path = "stabilityai/stable-diffusion-xl-base-1.0"
model_id_or_path = "stabilityai/stable-diffusion-xl-refiner-1.0"

# Pipeline.
# pipe = StableDiffusionXLImg2ImgPipeline.from_pretrained(
#     model_id_or_path,
#     torch_dtype=torch.float16,
#     variant="fp16",
#     use_safetensors=True,
# )
pipe = StableDiffusionXLImg2ImgPipeline.from_pretrained(
    "stabilityai/stable-diffusion-xl-refiner-1.0", 
    torch_dtype=torch.float16
)

# Device.
# device = "cpu"
# device = "mps" # macOS
device = "cuda" # NVIDIA
pipe = pipe.to(device)

# Image and prompt input.
print("Loading source image")
init_image = Image.open("source_image.png").convert("RGB").resize((512, 512))
nameOfCharacter = "batman"
prompt = "2 close up images of " + nameOfCharacter + "'s face, 1 profile image and 1 looking front on, plain background"

# Generation.
print("prompt: ", prompt)
print('generating...')
image = pipe(prompt=prompt, image=init_image, target_size=(256,256), strength=0.92, guidance_scale=7.5).images[0]

print("Finished")
image.save("gen.png")