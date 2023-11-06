import replicate
import os
import time
from concurrent.futures import ProcessPoolExecutor
from urllib.request import urlretrieve
from imgur_python import Imgur
import zipfile

class AttrDict(dict):
    def __init__(self, *args, **kwargs):
        super(AttrDict, self).__init__(*args, **kwargs)
        self.__dict__ = self

if not os.environ["REPLICATE_API_TOKEN"]:
    raise Exception("REPLICATE_API_TOKEN environment variable not set.")


IMAGE_DIMS = 768

# Generates 2d model using SDXL, and saves it under data/2d/{character_name}/{prediction_id}.png
# Returns Replicate's hosted URL of the image.
def gen_2d(image=None, prompt="", negative_prompt="", mask_image=None, save_dir=""):
    model = replicate.models.get("stability-ai/sdxl")
    version = model.versions.get("c221b2b8ef527988fb59bf24a8b97c4561f1c671f73bd389f866bfb27c061316")
    _input = {
        "prompt": prompt,
        "negative_prompt": negative_prompt,
        "width": IMAGE_DIMS,
        "height": IMAGE_DIMS,
        "num_inference_steps": 25,
    }
    if image:
        _input["image"] = image
    if mask_image:
        _input["mask_image"] = mask_image
    prediction = replicate.predictions.create(
        version=version,
        input=_input
    )
    
    # Wait for prediction to generate.
    print('gen_2d id={} image={} mask={}'.format(prediction.id, image, mask_image))
    prediction.wait()

    # Output info.
    if prediction.status == "failed":
        print("Error:", prediction.error)
        return None
    
    print('gen_2d done')
    print('gen_2d', prediction.output)

    # Make dir if not already exist.
    character_directory = "data/2d/{}".format(save_dir)
    if not os.path.exists(character_directory): # this only works when it's single-thread.
        try:
            os.makedirs(character_directory)
        except FileExistsError as e:
            pass


    # Save the file.
    fname = "data/2d/{}/{}.png".format(save_dir, prediction.id)
    urlretrieve(prediction.output[0], fname)
    
    return prediction.output[0]


# Generates 3d model using DreamGaussian, and saves it under data/3d/{character_name}/{prediction_id}/
# Returns prediction_id.
def gen_3d(image="", save_dir=""):
    _input = {
        "image": image,
        "image_size": IMAGE_DIMS,
        # Defaults.
        "num_steps": 500,
        "num_refinement_steps": 50,
        "num_point_samples": 5000,
    }
    
    # Approach 1: public Replicate deployment.
    model = replicate.models.get("alaradirik/dreamgaussian")
    version = model.versions.get("44d1361ed7b4e46754c70de0d91334e79a1bc8bbe3e7ec18835691629de25305")
    prediction = replicate.predictions.create(
        version=version,
        input=_input
    )

    # Approach 2: private Replicate deployment.
    # deployment = replicate.deployments.get("liamzebedee/dreamgaussian-private")
    # prediction = deployment.predictions.create(
    #     input=_input
    # )
    
    # Wait for prediction to generate.
    start_time = time.time()
    print('gen_3d id={} image={}'.format(prediction.id, image))
    prediction.wait()

    elapsed_time = time.time() - start_time
    print('gen_3d:done id={} elapsed_time={}'.format(prediction.id, elapsed_time))

    # Output info.
    if prediction.status == "failed":
        print("Error:", prediction.error)
        return None
    
    print('gen_3d', prediction.output)
    print(prediction.output)
    # prediction = AttrDict(id="foo", output=['https://pbxt.replicate.delivery/UhbgHHpcCJ6KGh9JtaN12EBB7NaXUkrb1tIkmYM1ye3hJo4IA/mesh_files.zip', 'https://pbxt.replicate.delivery/wtNZfqPTHJ1NaSkPje2EopGwX5h6CiDLlN6kLrxVcKGFTQxRA/image.mp4'])

    # Make dir if not already exist.
    character_directory = "data/3d/{}".format(save_dir)
    if not os.path.exists(character_directory):
        os.makedirs(character_directory)
    
    # Save the files.
    print('gen_3d id={} downloading files'.format(prediction.id))
    assert len(prediction.output) == 2
    mesh_files_url, mp4_file_url = prediction.output
    mesh_fname = "data/3d/{}/{}_mesh.zip".format(save_dir, prediction.id)
    mp4_fname = "data/3d/{}/{}_video.mp4".format(save_dir, prediction.id)
    urlretrieve(mesh_files_url, mesh_fname)
    urlretrieve(mp4_file_url, mp4_fname)

    # Make a directory to extract to.
    print('gen_3d id={} extracting .zip'.format(prediction.id))
    directory_to_extract_to = "data/3d/{}/{}_mesh".format(save_dir, prediction.id)
    if not os.path.exists(directory_to_extract_to):
        os.makedirs(directory_to_extract_to)

    # Unzip the .zip.
    zip_ref = zipfile.ZipFile(mesh_fname, 'r')
    zip_ref.extractall(directory_to_extract_to)

    # Delete the .zip.
    os.remove(mesh_fname)

    return prediction.id
    # return prediction.output[0]


# Given a character name, generates a bunch of headshots (3D models of the head).
# Saves the 2d images and 3d models to disk.
# 
# Notes:
# - pipeline is (prompt, reference_image, mask_image) -> 2d image via SDXL -> 3d model via DreamGaussian.
# - this function runs 4 generations in parallel
# - generation is run remotely on Replicate's GPU servers
def generate_headshots(character):
    print(character)

    # image = open("data/inputs-2d/head.png", "rb")
    image_black_bg = 'https://i.imgur.com/B5C3aww.png'
    image_white_bg = "https://i.imgur.com/F13wrbW.png"
    image_style_ref1 = 'https://i.imgur.com/9XRBmNi.png'
    image_style_ref2 = "https://i.imgur.com/HO8gyJf.png"

    # validate character string is only a-Z and space.
    # this is because we use it as a folder name to save the assets under.
    # might make this more flexible in future, works for now.
    assert character.replace(" ", "").isalpha()

    prompt_1 = "{}'s face, facing camera (0° angle), headshot, cartoon colours, gta v style, hyperrealism, a photorealistic painting, hyper realistic face, (clear white background:5)".format(character)
    # prompt_1 = "{}'s face, facing camera (0° angle), headshot, cartoon colours, gta v style, hyperrealism, a photorealistic painting, hyper realistic face, (clear white background:5)".format(character)
    prompt_2 = "{}'s face, facing camera (0° angle), headshot, 3d render, cartoon colours, gta v style, hyperrealism, a photorealistic painting, hyper realistic face, (clear white background:5)".format(character)

    negative_prompt = "text, multicolored, black and white, signature, sig, (merged hands:1.3), (merged arms:1.3), merged, legs, merged bodyparts, BadDream, (UnrealisticDream:1.2), bad_prompt_version2, bad-artist bad-artist-anime, EasyNegative , longbody, lowres, bad anatomy, bad hands, missing fingers, extra digit, fewer digits, cropped, worst quality, low quality, multiple people"


    executor = ProcessPoolExecutor(max_workers=32)

    # 1. Generate a bunch of character profiles.
    inputs_2d = [
        # (image_style_ref1, prompt_1, negative_prompt, None),
        # (image_style_ref1, prompt_1, negative_prompt, None),
        # (image_style_ref2, prompt_2, negative_prompt, None),
        # (image_style_ref2, prompt_2, negative_prompt, None),

        # (image_style_ref1, prompt_1, negative_prompt, image_white_bg),
        # (image_style_ref1, prompt_1, negative_prompt, image_white_bg),
        # (image_style_ref2, prompt_2, negative_prompt, image_white_bg),
        # (image_style_ref2, prompt_2, negative_prompt, image_white_bg),

        (None, prompt_1, negative_prompt, image_white_bg, character),
        (None, prompt_1, negative_prompt, image_white_bg, character),
        (None, prompt_2, negative_prompt, image_white_bg, character),
        (None, prompt_2, negative_prompt, image_white_bg, character),

        # (image_style_ref1, prompt_1, negative_prompt, None),
        # (image_style_ref1, prompt_1, negative_prompt, None),
        # (image_style_ref2, prompt_2, negative_prompt, None),
        # (image_style_ref2, prompt_2, negative_prompt, None),

        # (None, prompt_1, negative_prompt, None),
        # (None, prompt_1, negative_prompt, None),
        # (None, prompt_2, negative_prompt, None),
        # (None, prompt_2, negative_prompt, None),
    ]
    
    image_2d_results = executor.map(gen_2d, *zip(*inputs_2d))
    image_2d_results = list(image_2d_results)
    print(image_2d_results)

    # 2. Generate 3d models for all the 2d images.
    inputs_3d = [
        (image_2d_url, character) for image_2d_url in image_2d_results
    ]

    image_3d_results = executor.map(gen_3d, *zip(*inputs_3d))
    image_3d_results = list(image_3d_results)
    print(image_3d_results)

    return image_3d_results


def main():
    generate_headshots('walter white')


if __name__ == '__main__':
    main()
