Hello welcome to my hell.
ahh so yeah have a look around.
I'm not gonna pay for you to play around with this so youre gonna need ur own api keys and stuff.
you neeed....
Chatgpt api key.
Fake you username and password.

these are to be stored in your computers environment variables 
to add them go windows > search environmen variables > then click the button at the bottom that says Evironment Variabels...

make sure theyre user variables not system variables.
then add your keys with these names

OPENAI_API_KEY
FAKE_YOU_USERNAME_OR_EMAIL
FAKE_YOU_PASSWORD

and hit run and magic, i guess.
if you wanna do your own unique prompt, in the WholeThingManager code on line 93ish theres a line to change, not the sexiest way of doing that but fuck you i do what i want.

ooh also if you want to stream it, youll first need to install node.js (jsut google it its super easy) then you need to install some depenecies so get a terminal happening, 
navigate to the folder where the main.js shit is. (should be the one with assets in it). then in the terminal type: npm i 
thatll install some shit probably. 
then type node main.js 
and thatll run the bitch, in the main.js thing you will need to change the channel_id to whatever channel you want. use this to find the channel id https://commentpicker.com/youtube-channel-id.php
ahh yeah thatll do i guess. if your not on windows go fuck yourself.

also youll need to enable youtube chat by setting the usingYoutubeChatStuff to true in the youtube chat manager in the inspector. not sure if thats on or off by default but the toggle exists so there.


