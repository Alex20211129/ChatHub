function formatTime(sentAt) {
    if (!sentAt) return "";

    // 後端存的 SentAt 一律是 UTC，但序列化出來的字串不一定會帶時區標記（Z），
    // 沒有的話這裡補上，確保 new Date(...) 是照 UTC 解析，而不是被瀏覽器誤判成本地時間。
    const hasTimezone = /Z$|[+-]\d{2}:\d{2}$/.test(sentAt);
    const date = new Date(hasTimezone ? sentAt : `${sentAt}Z`);

    return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}

export function appendChatMessage(messagesEl, displayName, text, sentAt) {
    const wrapper = document.createElement("div");
    wrapper.className = "chat-message";

    const author = document.createElement("span");
    author.className = "chat-author";
    author.textContent = displayName;

    const time = document.createElement("span");
    time.className = "chat-time";
    time.textContent = formatTime(sentAt);

    wrapper.appendChild(author);
    wrapper.appendChild(document.createTextNode(text));
    wrapper.appendChild(time);

    messagesEl.appendChild(wrapper);
    messagesEl.scrollTop = messagesEl.scrollHeight;
}

export function appendSystemMessage(messagesEl, text) {
    const wrapper = document.createElement("div");
    wrapper.className = "chat-message system-message";
    wrapper.textContent = text;
    messagesEl.appendChild(wrapper);
    messagesEl.scrollTop = messagesEl.scrollHeight;
}

export function renderOnlineList(onlineListEl, displayNames) {
    onlineListEl.innerHTML = "";
    for (const name of displayNames) {
        const li = document.createElement("li");
        li.textContent = name;
        onlineListEl.appendChild(li);
    }
}

// ------------------------------------------------------------------
// 以下是同樣三個函式改用 jQuery 寫法的對照版本，純粹參考用，
// 沒有被呼叫、也沒有實際載入 jQuery，不影響現有功能。
// ------------------------------------------------------------------
//
// function appendChatMessageJQuery(messagesEl, displayName, text, sentAt) {
//     const $wrapper = $("<div>").addClass("chat-message");
//     $("<span>").addClass("chat-author").text(displayName).appendTo($wrapper);
//     $wrapper.append(document.createTextNode(text));
//     $("<span>").addClass("chat-time").text(formatTime(sentAt)).appendTo($wrapper);
//     $(messagesEl).append($wrapper);
//     $(messagesEl).scrollTop(messagesEl.scrollHeight);
// }
//
// function appendSystemMessageJQuery(messagesEl, text) {
//     $("<div>")
//         .addClass("chat-message system-message")
//         .text(text)
//         .appendTo(messagesEl);
//     $(messagesEl).scrollTop(messagesEl.scrollHeight);
// }
//
// function renderOnlineListJQuery(onlineListEl, displayNames) {
//     const $list = $(onlineListEl).empty();
//     displayNames.forEach((name) => {
//         $("<li>").text(name).appendTo($list);
//     });
// }
