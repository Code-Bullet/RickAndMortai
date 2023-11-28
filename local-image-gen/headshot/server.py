from flask import Flask, request, jsonify, send_from_directory
from pathlib import Path
import os
import requests
from generate import generate_headshots
import config

app = Flask(__name__)

# External URL of the server.
# This is used for hosting/linking to images in JSON API replies.
server_port = 10001
server_base_url = "http://0.0.0.0:{server_port}"


# Generate headshots.
# Input: {character: "walter white"}
# Output: {generation_ids: ["3m7v6stb7lbvhysbnykluzteyq", "gjveqa3bnufcidlumyactt5gd4"]}
# Error: {error: "Error generating headshots - {err}}
@app.route('/v1/character-heads/generate', methods=['POST'])
def generate():
    character_name = request.json.get('character')

    # Testing:
    # if character_name == 'trump':
    #     # zfag623bhpj2dzxiqjogsjkf7u
    #     test_ids = ['av4y6plbhuph5rhgmckpvrkbdy', 'w6cqlj3bhuf6kjyd2rvmhyylj4', '5bjain3bl6pi5i3datgprlc33q']
    #     return jsonify({'generation_ids': test_ids}), 200
        
    if character_name == 'Dutch man':
        test_ids = ['lhinfidbdsy3ay32qnedgwgskq', 'nuedrilbhxjvyo5bhn7ndycjbu', 'pejzlqlb2aviwzqvktw47nrntq', 'fhnfrslb6zrrb7mr55pri5rvwi']
        return jsonify({'generation_ids': test_ids}), 200

    
    if not character_name or not isinstance(character_name, str) or len(character_name) == 0:
        return jsonify({'error': 'Missing character_name.'}), 400
    
    print('generate', character_name)

    try:
        generation_ids = generate_headshots(character_name)
    except Exception as err:
        return jsonify({'error': 'Error generating headshots - {}.'.format(err)}), 500
    
    return jsonify({'generation_ids': generation_ids}), 200



if __name__ == '__main__':
    print("Running character headshot generation server at {}".format(server_base_url))
    app.run(host='0.0.0.0', port=server_port)
