

from flask import Flask, request, jsonify, send_from_directory
from pathlib import Path
import os
from models import get_sdxl, infer_sdxl, get_sd, infer_sd
import requests

app = Flask(__name__)

# `version` is the model to use for the prediction.
# https://replicate.com/stability-ai/sdxl/versions/8beff3369e81422112d93b89ca01426147de542cd4684c244b673b105188fe5f
model_version_to_repo = {
    '8beff3369e81422112d93b89ca01426147de542cd4684c244b673b105188fe5f': "stability-ai/sdxl",
}

# URL -> local path.
input_image_cache = {}
predictions = {}
prediction_counter = 0

# The image model we've loaded.
# Tuple of (pipeline instance, infer function)
ai_model = None

# External URL of the server.
# This is used for hosting/linking to images in JSON API replies.
server_base_url = "http://0.0.0.0:8080"

data_dir = os.path.join(Path().absolute(), 'data/')
print("Data dir ", data_dir)


@app.route('/v1/predictions', methods=['POST'])
def predict():
    global prediction_counter, ai_model, server_base_url, data_dir, input_image_cache, predictions, model_version_to_repo
    
    # 
    # Request body.
    # 
    version = request.json.get('version')
    input_data = request.json.get('input', {})

    prompt = input_data.get('prompt')
    width = input_data.get('width')
    height = input_data.get('height')
    prompt_strength = input_data.get('prompt_strength')
    image_url = input_data.get('image')

    # Validate params.
    if not version:
        return jsonify({'error': 'Missing version.'}), 400
    if not prompt:
        return jsonify({'error': 'Missing prompt.'}), 400
    if not width:
        return jsonify({'error': 'Missing width.'}), 400
    if not height:
        return jsonify({'error': 'Missing height.'}), 400
    if not prompt_strength:
        return jsonify({'error': 'Missing prompt_strength.'}), 400
    if not image_url:
        return jsonify({'error': 'Missing image.'}), 400
    

    # Headers.
    # Ignored: Authorization, Content-Type.

    # Create new prediction.
    prediction_id = prediction_counter
    prediction_counter += 1
    predictions[prediction_id] = {
        'id': prediction_id,
        'input': input_data,
        'status': 'pending',
        'error': None,
        'output': None,
    }

    # Start the inference using the AI model.
    if version not in model_version_to_repo:
        predictions[prediction_id]['status'] = 'error'
        predictions[prediction_id]['error'] = 'Model not found for version.'
        return jsonify(predictions[prediction_id])
    
    # 
    # Download the image to the cache dir.
    # 

    print("Downloading template image: "+image_url)
    
    # Specify the path to save the image
    input_image_filename = "{}_source.png".format(prediction_id)
    input_image_path = ""

    if image_url in input_image_cache:
        # Use the cached image.
        input_image_path = input_image_cache[image_url]
    else:
        # Download it.
        input_image_path = os.path.join(data_dir, input_image_filename)
        response = requests.get(image_url)

        if response.status_code == 200:
            # Open the file and write the image content to it
            with open(input_image_path, 'wb') as image_file:
                image_file.write(response.content)
                print("Image downloaded to: ", input_image_path)
        else:
            print("Error downloading image: ", image_url)
            predictions[prediction_id]['status'] = 'error'
            predictions[prediction_id]['error'] = 'Error downloading image.'
            return jsonify(predictions[prediction_id])
        
        # Cache the image.
        input_image_cache[image_url] = input_image_path
    

    # Now run the local AI model.
    print("Running inference")
    # output_image = infer_sdxl(input_image_path, prompt, width, height, prompt_strength, ai_model)
    (pipe, infer) = ai_model
    output_image = infer(input_image_path, prompt, width, height, prompt_strength, pipe)

    print("Done inference")

    # Save the output image.
    output_image_filename = "{}_output.png".format(prediction_id)
    output_image_path = os.path.join(data_dir, output_image_filename)
    output_image.save(output_image_path)

    # Update the prediction.
    predictions[prediction_id]['status'] = 'complete'
    predictions[prediction_id]['output'] = {
        'image': "{}/custom/images/{}".format(server_base_url, output_image_filename),
    }

    return jsonify(predictions[prediction_id])
    

@app.route('/custom/images/<string:filename>', methods=['GET'])
def download_image(filename):
    return send_from_directory(data_dir, filename)


@app.route('/v1/predictions/<string:id>', methods=['GET'])
def get_prediction(_id):
    global predictions
    if _id not in predictions:
        return jsonify({'error': 'Prediction not found.'}), 404
    return jsonify(predictions[_id])


def load_ai_model():
    global ai_model

    print("Loading AI model...")

    # StableDiffusion base.
    ai_model = (get_sd(), infer_sd)
    
    # StableDiffusion XL.
    # ai_model = get_sdxl(), infer_sdxl

    print(ai_model)


if __name__ == '__main__':
    print("Loading AI model")
    load_ai_model()

    # 1. Load the AI model at start.
    # 2. Receive the POST request for a prediction.
    # 3. Create a new prediction request (with ID).
    # 4. Handle the prediction in background:
    #   1. Download the source image to a local cache.
    #   2. Run the model on the source image, async, in a separate thread.
    #   3. Save image to output directory.
    # 5. Serve requests on the /v1/predictions/<id> endpoint.
    #   1. If the prediction is not ready, return a 404.
    #   2. If the prediction is ready, return the URL to the file.
    # 6. Serve generated image files on a custom endpoint.
    #   1. /custom/images/<filename>

    print("Running inference server at {}".format(server_base_url))
    app.run(host='0.0.0.0', port=8080)
