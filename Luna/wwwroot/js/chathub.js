//create connection
var connectionChatHub = new signalR.HubConnectionBuilder().withUrl("/hubs/chat").build();

//connect to method that hub invokes aka receive notifications from hub
connectionChatHub.on("ReceiveMessage", function (sender, message, time, consultantId) {
    var encodedUser = $("<div />").text(sender).html();
    var encodedMsg = $("<div />").text(message).html();
    var encodedTime = $("<div />").text(time).html();
    var messageHtml;

    //Nguoi dung gui tin nhan
    if (sender != consultantId) {
        messageHtml = `
        <div class="my-message">
            <div class="d-flex justify-content-end">
                <p class="small mb-1 text-muted">${encodedTime}</p>
            </div>
            <div class="d-flex flex-row justify-content-end mb-4 pt-1">
                <div>
                    <p class="small p-2 me-3 mb-3 text-white rounded-3 bg-warning">
                        ${encodedMsg}
                    </p>
                </div>
                <img src="https://mdbcdn.b-cdn.net/img/Photos/new-templates/bootstrap-chat/ava6-bg.webp"
                     alt="avatar 1" style="width: 45px; height: 100%;">
            </div>
        </div>`;
        $("#chatBox").append(messageHtml);

        //Neu khong phai nguoi nhan thi khong show tin nhan
        var receiverId = document.getElementById("receiverId").value;
        if (sender == receiverId) {
            messageHtml = `
                <div class="d-flex flex-row justify-content-start">
                    <img src="https://mdbcdn.b-cdn.net/img/Photos/new-templates/bootstrap-chat/ava6-bg.webp"
                         alt="avatar 1" style="width: 45px; height: 100%;">
                    <div>
                        <p class="small p-2 ms-3 mb-1 rounded-3" style="background-color: #f5f6f7;">
                            ${encodedMsg}
                        </p>
                        <p class="small ms-3 mb-3 rounded-3 text-muted float-end">${encodedTime}</p>
                    </div>
                </div>`;
            $("#chatBoxStaff").append(messageHtml);
        }
        
    }
    //nhan vien tu van gui tin nhan
    else {
        messageHtml = `
        <div class="d-flex flex-row justify-content-end">
                    <div>
                        <p class="small p-2 me-3 mb-1 text-white rounded-3 bg-primary">
                           ${encodedMsg}
                        </p>
                        <p class="small me-3 mb-3 rounded-3 text-muted float-end">${encodedTime}</p>
                    </div>
                    <img src="https://mdbcdn.b-cdn.net/img/Photos/new-templates/bootstrap-chat/ava1-bg.webp"
                         alt="avatar 1" style="width: 45px; height: 100%;">
                </div>`;
        $("#chatBoxStaff").append(messageHtml);
        messageHtml = `
        <div class="my-message">
            <div class="d-flex justify-content-start">
                        
                            <p class="small mb-1 text-muted">${encodedTime}</p>
                        </div>
                        <div class="d-flex flex-row justify-content-start">
                            <img src="https://mdbcdn.b-cdn.net/img/Photos/new-templates/bootstrap-chat/ava5-bg.webp"
                                 alt="avatar 1" style="width: 45px; height: 100%;">
                            <div>
                                <p class="small p-2 ms-3 mb-3 rounded-3" style="background-color: #f5f6f7;">
                                    ${encodedMsg}
                                </p>
                            </div>
                        </div>
        </div>`;
        $("#chatBox").append(messageHtml);
    }


});


//invoke hub methods aka send notification to hub
function loadMessage(message, userId, consultantId) {
    if (userId !== consultantId) {
        connectionChatHub.invoke("SendMessageToStaff", userId, message);
    }
    else {
        var receiverId = document.getElementById("receiverId").value;
        connectionChatHub.invoke("SendMessageToUser", receiverId, message);
    }
}

//start connection
function fulfilled() {
    document.getElementById("sendButton").addEventListener('click', function (event) {
        var messageInput = document.getElementById("message");
        var userId = document.getElementById("userId").value;
        var consultantId = document.getElementById("consultantId").value;       
        var message = messageInput.value;
        if (message.trim() !== "") {
            loadMessage(message, userId, consultantId);
            messageInput.value = "";
        }
        event.preventDefault();
    })
}
function rejected() {

}
connectionChatHub.start().then(fulfilled, rejected);