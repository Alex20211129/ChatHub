import { appendChatMessage, appendSystemMessage, renderOnlineList } from "./chat-render.js";

const statusEl = document.getElementById("status");
const joinPanel = document.getElementById("joinPanel");
const chatPanel = document.getElementById("chatPanel");
const displayNameInput = document.getElementById("displayNameInput");
const joinButton = document.getElementById("joinButton");
const messagesEl = document.getElementById("messages");
const messageInput = document.getElementById("messageInput");
const sendButton = document.getElementById("sendButton");
const onlineListEl = document.getElementById("onlineList");

function setStatus(text, statusClass) {
    statusEl.textContent = text;
    statusEl.className = `status-badge ${statusClass}`;
}

function setChatInteractive(interactive) {
    messageInput.disabled = !interactive;
    sendButton.disabled = !interactive;
}

displayNameInput.value = displayNameInput.dataset.defaultName ?? "";

let socket = null;
let hasJoined = false;
let lastDisplayName = null;
let isUnloading = false;

window.addEventListener("pagehide", () => {
    isUnloading = true;
});

function connect() {
    const protocol = window.location.protocol === "https:" ? "wss:" : "ws:";
    socket = new WebSocket(`${protocol}//${window.location.host}/ws/chat`);

    socket.addEventListener("open", () => {
        if (hasJoined) {
            // 重連成功，用記住的暱稱自動重新加入，不用再跳表單
            socket.send(JSON.stringify({ type: "join", displayName: lastDisplayName }));
        } else {
            setStatus("已連線，請輸入暱稱", "status-connected");
            joinPanel.style.display = "block";
        }
    });

    socket.addEventListener("close", () => {
        setStatus(hasJoined ? "連線已中斷，重新連線中..." : "連線已中斷", "status-disconnected");
        setChatInteractive(false);
        scheduleReconnect();
    });

    socket.addEventListener("message", (event) => {
        const data = JSON.parse(event.data);
        switch (data.type) {
            case "Joined":
                hasJoined = true;
                lastDisplayName = data.displayName;
                joinPanel.style.display = "none";
                chatPanel.style.display = "block";
                setChatInteractive(true);
                setStatus("已連線", "status-connected");
                break;
            case "History":
                for (const message of data.messages) {
                    appendChatMessage(messagesEl, message.displayName, message.text, message.sentAt);
                }
                break;
            case "SystemMessage":
                appendSystemMessage(messagesEl, data.text);
                break;
            case "OnlineList":
                renderOnlineList(onlineListEl, data.displayNames);
                break;
            case "ReceiveMessage":
                appendChatMessage(messagesEl, data.message.displayName, data.message.text, data.message.sentAt);
                break;
        }
    });
}

function scheduleReconnect() {
    if (isUnloading) {
        return; // 頁面真的要關掉了，不用重連
    }

    if (document.visibilityState === "visible") {
        // 分頁目前還在前景（例如單純網路抖動），3 秒後自動重試一次
        setTimeout(() => {
            if (isUnloading || socket.readyState !== WebSocket.CLOSED) {
                return;
            }
            setStatus("重新連線中...", "status-connecting");
            connect();
        }, 3000);
    }
    // 分頁在背景的話先不主動重試，等使用者切回來（visibilitychange）才連
}

document.addEventListener("visibilitychange", () => {
    if (isUnloading) {
        return;
    }
    if (document.visibilityState === "visible" && socket.readyState === WebSocket.CLOSED) {
        setStatus("重新連線中...", "status-connecting");
        connect();
    }
});

connect();

joinButton.addEventListener("click", () => {
    const displayName = displayNameInput.value.trim();
    socket.send(JSON.stringify({ type: "join", displayName }));
});

sendButton.addEventListener("click", () => {
    if (socket.readyState !== WebSocket.OPEN) {
        setStatus("連線已中斷，重新連線中...", "status-disconnected");
        return;
    }

    const text = messageInput.value.trim();
    if (!text) return;

    socket.send(JSON.stringify({ type: "message", text }));
    messageInput.value = "";
});

messageInput.addEventListener("keydown", (event) => {
    if (event.key === "Enter") {
        sendButton.click();
    }
});

// ------------------------------------------------------------------
// jQuery 寫法對照（純參考，這個檔案本身沒有載入/使用 jQuery）：
// WebSocket 本身跟 jQuery 無關，jQuery 能簡化的只有「選取 DOM 元素」
// 跟「綁定事件」這兩件事，下面示範同樣的選取/事件綁定改用 jQuery 的寫法。
//
// const $status = $("#status");
// const $joinPanel = $("#joinPanel");
// const $displayNameInput = $("#displayNameInput");
//
// $displayNameInput.val($displayNameInput.data("default-name") ?? "");
//
// $joinPanel.hide(); // 等同 joinPanel.style.display = "none"
// $joinPanel.show(); // 等同 joinPanel.style.display = "block"
//
// $("#joinButton").on("click", () => {
//     const displayName = $displayNameInput.val().trim();
//     socket.send(JSON.stringify({ type: "join", displayName }));
// });
//
// $("#messageInput").on("keydown", (event) => {
//     if (event.key === "Enter") {
//         $("#sendButton").trigger("click");
//     }
// });
// ------------------------------------------------------------------
