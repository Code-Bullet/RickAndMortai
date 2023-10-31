import torch
from diffusers import StableDiffusionPipeline
import numpy as np
import requests
import torch
from PIL import Image
from io import BytesIO
from diffusers import StableDiffusionImg2ImgPipeline, StableDiffusionXLImg2ImgPipeline
from diffusers.utils import load_image
import os


def get_sd():
    model_id_or_path = "runwayml/stable-diffusion-v1-5"
    pipe = StableDiffusionImg2ImgPipeline.from_pretrained(
        model_id_or_path
    )
    # Device.
    device = "mps" # macOS
    # device = "cuda" # NVIDIA
    pipe = pipe.to(device)

    # Optimizations.
    pipe.enable_attention_slicing()

    return pipe

def infer_sd(init_image_path, prompt, width, height, strength, pipe):
    # Image and prompt input.
    print("Loading source image")
    init_image = Image.open(init_image_path).convert("RGB")
    init_image = init_image.resize((768, 512))

    # Generation.
    print("prompt: ", prompt)
    print('generating...')
    
    num_inference_steps = 2 if os.environ['TESTING'] else 50
    # guidance_scale = 1.1 if os.environ['TESTING'] else 7.5
    image = pipe(prompt=prompt, image=init_image, strength=strength, num_inference_steps=num_inference_steps, guidance_scale=7.5).images[0]
    print("Finished")
    return image


def get_sdxl():
    # Model.
    model_id_or_path = "runwayml/stable-diffusion-v1-5"

    # Pipeline.
    pipe = StableDiffusionImg2ImgPipeline.from_pretrained(model_id_or_path, torch_dtype=torch.float16)
    # pipe = StableDiffusionXLImg2ImgPipeline.from_pretrained(
    #     model_id_or_path,
    #     torch_dtype=torch.float16,
    #     variant="fp16",
    #     use_safetensors=True,
    # )

    # Device.
    device = "mps" # macOS
    # device = "cuda" # NVIDIA
    pipe = pipe.to(device)

    # Optimizations.
    # pipe.enable_attention_slicing()

    return pipe

def infer_sdxl(init_image_path, prompt, width, height, strength, pipe):
    # Image and prompt input.
    print("Loading source image")
    init_image = Image.open(init_image_path).convert("RGB")
    init_image = init_image.resize((768, 512))

    # Generation.
    print("prompt: ", prompt)
    print('generating...')
    image = pipe(prompt=prompt, image=init_image, strength=strength, width=width, height=height).images[0]
    # image = pipe(prompt=prompt, image=init_image, strength=0.92, guidance_scale=7.5).images[0]
    print("Finished")
    # image.save("gen.png")
    return image

