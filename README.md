# Peer

How use it?

1. Get APP_KEY, APP_SECRET:</br>
Create Dropbox app with select App Folder: https://dropbox.com/developers/apps/create

2. Get Authorize Code:</br>
Set APP_KEY in url: https://dropbox.com/oauth2/authorize?client_id=<APP_KEY>&token_access_type=offline&response_type=code

3. Get Refresh Token:</br>
Post to https://api.dropboxapi.com/oauth2/token with APP_KEY and APP_SECRET:
```pyton
import requests
ACCESS_CODE_GENERATED_FROM_OAUTH2 = ""
APP_KEY = ""
APP_SECRET = ""
data = f'code={ACCESS_CODE_GENERATED_FROM_OAUTH2}&grant_type=authorization_code'
response = requests.post('https://api.dropboxapi.com/oauth2/token', data=data, auth=(APP_KEY, APP_SECRET))
print(json.dumps(json.loads(response.text), indent=2))
```
