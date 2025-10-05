from flask import Flask, request, send_file, current_app
import os

app = Flask(__name__)

@app.after_request
def after_request(response):
    response.headers.add('Access-Control-Allow-Origin', '*')
    response.headers.add('Access-Control-Allow-Headers', 'Content-Type,Authorization,Cache-Control')
    response.headers.add('Access-Control-Allow-Methods', 'GET,PUT,POST,DELETE,OPTIONS')
    return response

@app.route('/peer/<path:filename>')
def static_file(filename):
    return send_file(os.path.join('static', os.path.join('peer', filename)))

@app.route('/peer/upload', methods=['POST'])
def upload():
    file = request.files['file']
    if file is None:
        return "", 400
    upload_folder = os.path.join(current_app.root_path, 'static', 'peer')
    os.makedirs(upload_folder, exist_ok=True)
    i = 0
    while True:
        file_id = f"{i}_{file.filename}"
        file_path = os.path.join(upload_folder, file_id)
        i += 1
        if os.path.exists(file_path):
            continue
        try:
            file.save(file_path)
            return f"{request.host_url.rstrip('/')}/peer/{file_id}", 200
        except Exception:
            if os.path.exists(file_path):
                continue
            return "", 500

@app.route('/peer/index')
def index():
    return """<!DOCTYPE html><html><head><style>body { background:#121212; } input, a { font-size:40px; display:block; color:#fff; }</style>
         </head><body><input type="file" id="in" onchange="upload();" />
          <script>async function upload() {
            const fd = new FormData(); fd.append("file", document.getElementById("in").files[0]);
            const url = await (await fetch(location.origin + "/peer/upload", { method: "POST", body: fd })).text();
            document.body.innerHTML += '<a target="_blank" href="' + url + '">' + url + "</a>";
          }</script></body></html>""", 200

if __name__ == '__main__':
    app.run(debug=True)