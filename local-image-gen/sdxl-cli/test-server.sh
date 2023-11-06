curl -s -X POST \
  -H "Content-Type: application/json" \
  -d '{"version": "8beff3369e81422112d93b89ca01426147de542cd4684c244b673b105188fe5f", "input": {"prompt": "An astronaut riding a rainbow unicorn","width":512,"height":256,"image":"https://i.imgur.com/3fm0Z0o.png","prompt_strength":0.92}}' \
  -H "Authorization: Token Foo" \
  "http://0.0.0.0:8080/v1/predictions"


