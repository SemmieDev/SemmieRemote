"use strict";
let connectDiv = document.getElementById("connect");
let ipInput = document.getElementById("ipInput");
let buttonConnect = document.getElementById("buttonConnect");
let errorText = document.getElementById("errorText");
let streamDiv = document.getElementById("stream");
let streamFrame = document.getElementById("streamFrame");
let ws;
let sensitivity = 0.0003;
let pitch = 0;
let yaw = 0;
ipInput.value = localStorage.getItem("savedIp") ?? "";
function showStream(ip) {
    if (ip == null) {
        buttonConnect.disabled = false;
        connectDiv.hidden = false;
        streamDiv.hidden = true;
        streamFrame.src = "about:blank";
        return;
    }
    connectDiv.hidden = true;
    streamDiv.hidden = false;
    streamFrame.src = `https://${ip}:8889/vrcbuddycam?controls=false&muted=false&disablepictureinpicture=true`;
}
buttonConnect.addEventListener("click", event => {
    buttonConnect.disabled = true;
    errorText.innerText = "";
    let ip = ipInput.value;
    localStorage.setItem("savedIp", ip);
    ws = new WebSocket(`wss://${ip}:6854`);
    let connectionErrorListener = (event) => {
        buttonConnect.disabled = false;
        errorText.innerText = `Failed to connect`;
    };
    ws.addEventListener("error", connectionErrorListener);
    ws.addEventListener("open", event => {
        showStream(ip);
        ws.removeEventListener("error", connectionErrorListener);
        ws.addEventListener("error", event => {
            showStream(null);
            errorText.innerText = "Disconnected due to a connection error";
        });
    });
});
streamDiv.addEventListener("click", async (event) => {
    await streamFrame.requestPointerLock({
        unadjustedMovement: true
    });
});
streamDiv.addEventListener("pointermove", event => {
    pitch += event.movementY * sensitivity;
    pitch -= Math.floor(pitch);
    yaw += event.movementX * sensitivity;
    yaw -= Math.floor(yaw);
    ws.send(JSON.stringify({
        type: "rotation",
        pitch: pitch,
        yaw: yaw
    }));
});
function onKey(event, isDown) {
    if (streamDiv.hidden || event.repeat)
        return;
    ws.send(JSON.stringify({
        type: "key",
        key: event.code,
        isDown: isDown
    }));
}
document.body.addEventListener("keyup", event => onKey(event, false));
document.body.addEventListener("keydown", event => onKey(event, true));
//# sourceMappingURL=script.js.map