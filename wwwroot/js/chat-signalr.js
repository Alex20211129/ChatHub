import * as signalR from "https://cdn.jsdelivr.net/npm/@microsoft/signalr@8.0.7/+esm";
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

displayNameInput.value = displayNameInput.dataset.defaultName ?? "";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect()
    .build();

connection.on("Joined", (displayName, isAuthenticated) => {
    joinPanel.style.display = "none";
    chatPanel.style.display = "block";
});

connection.on("History", (messages) => {
    for (const message of messages) {
        appendChatMessage(messagesEl, message.displayName, message.text, message.sentAt);
    }
});

connection.on("SystemMessage", (text) => {
    appendSystemMessage(messagesEl, text);
});

connection.on("OnlineList", (displayNames) => {
    renderOnlineList(onlineListEl, displayNames);
});

connection.on("ReceiveMessage", (message) => {
    appendChatMessage(messagesEl, message.displayName, message.text, message.sentAt);
});

connection.onreconnecting(() => {
    setStatus("重新連線中...", "status-connecting");
});

connection.onreconnected(() => {
    setStatus("已連線", "status-connected");
});

connection.onclose(() => {
    setStatus("連線已中斷", "status-disconnected");
});

connection.start()
    .then(() => {
        setStatus("已連線，請輸入暱稱", "status-connected");
        joinPanel.style.display = "block";
    })
    .catch(() => {
        setStatus("連線失敗", "status-disconnected");
    });

joinButton.addEventListener("click", () => {
    const displayName = displayNameInput.value.trim();
    connection.invoke("JoinAsync", displayName).catch(console.error);
});

sendButton.addEventListener("click", () => {
    const text = messageInput.value.trim();
    if (!text) return;

    connection.invoke("SendMessageAsync", text).catch(console.error);
    messageInput.value = "";
});

messageInput.addEventListener("keydown", (event) => {
    if (event.key === "Enter") {
        sendButton.click();
    }
});

// ------------------------------------------------------------------
// jQuery 寫法對照（純參考，這個檔案本身沒有載入/使用 jQuery）：
// SignalR 的 connection/invoke API 跟 jQuery 無關，一樣只有「選取
// DOM 元素」跟「綁定事件」這兩件事可以改寫成 jQuery 風格。
//
// const $status = $("#status");
// const $joinButton = $("#joinButton");
//
// $joinButton.on("click", () => {
//     const displayName = $("#displayNameInput").val().trim();
//     connection.invoke("JoinAsync", displayName).catch(console.error);
// });
//
// $("#messageInput").on("keydown", (event) => {
//     if (event.key === "Enter") {
//         $("#sendButton").trigger("click");
//     }
// });
// ------------------------------------------------------------------
