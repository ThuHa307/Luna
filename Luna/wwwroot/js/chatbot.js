document.getElementById('about-us').onclick = function () {
    showMessage(document.getElementById('about-us').textContent, 'Luna là chuỗi khách sạn cao cấp với mong muốn đem lại cho bạn những phút giây thư giãn thật tuyệt vời và đáng nhớ.');
};
document.getElementById('best-seller').onclick = function () {
    showMessage(document.getElementById('best-seller').textContent, 'Hiện nay, Opera là loại phòng được đánh giá cao nhất của Luna với không gian rộng rãi và thoáng mát, hứa hẹn sẽ đem đến cho bạn những trải nghiệm tuyệt vời!');
};
document.getElementById('services').onclick = function () {
    showMessage(document.getElementById('services').textContent, 'Các dịch vụ hiện có của Luna bao gồm: Ăn uống, Gym, Party,... Bạn có thể vảo phần "Dịch vụ" để có thể xem chi tiết.');
};

document.getElementById('policy').onclick = function () {
    showMessage(document.getElementById('policy').textContent, 'Luna hỗ trợ hoàn trả 90% phí đặt phòng đã thanh toán trước vào ví tài khoản cá nhân của bạn.');
};
document.getElementById('contact-staff').onclick = function () {
    showMessage(document.getElementById('contact-staff').textContent, 'Bạn có thể liên hệ trực tiếp cho chúng tôi qua hotline 0123456789. Hoặc đăng nhập tài khoản của Luna để có thể nhắn tin trao đổi với nhân viên tư vấn.');
};

function showMessage(option, message) {
    const optionsContainer = document.getElementById('options-container');
    optionsContainer.remove();
    const responseContainer = document.getElementById('response-container');
    const responseHtml = `
                <div class="d-flex flex-row justify-content-end mb-4 pt-1">
                    <div>
                        <p class="small p-2 me-3 mb-3 text-white rounded-3" style="background-color: #4251ddcf;">
                            ${option}
                        </p>
                    </div>
                    <img src="https://mdbcdn.b-cdn.net/img/Photos/new-templates/bootstrap-chat/ava6-bg.webp"
                         alt="avatar 1" style="width: 45px; height: 100%;">
                </div>
            `;
    const answerHtml = `
                <div>
                    <div class="d-flex flex-row justify-content-start">
                        <img src="https://img.freepik.com/free-vector/chatbot-chat-message-vectorart_78370-4104.jpg"
                             alt="avatar 1" style="width: 45px; height: 100%;">
                        <div>
                            <p class="small p-2 ms-3 mb-3 rounded-3" style="background-color: #f5f6f7;">
                                ${message}
                            </p>
                        </div>
                    </div>
                    <div id="options-container" class="options">
                        <p id="about-us" class="option">Về chúng tôi</p>
                        <p id="best-seller" class="option">Best Seller</p>
                        <p id="services" class="option">Các dịch vụ của Luna</p>
                        <p id="policy" class="option">Chính sách hủy đặt phòng</p>
                        <p id="contact-staff" class="option">Liên hệ với nhân viên tư vấn</p>
                    </div>
                </div>
            `;

    responseContainer.innerHTML += responseHtml;
    responseContainer.scrollTo(0, responseContainer.scrollHeight);

    setTimeout(() => {
        var thinking = 
            `
                <div id="thinking">
                    <div class="d-flex flex-row justify-content-start">
                        <img src="https://img.freepik.com/free-vector/chatbot-chat-message-vectorart_78370-4104.jpg"
                             alt="avatar 1" style="width: 45px; height: 100%;">
                        <div>
                            <p class="small p-2 ms-3 mb-3 rounded-3" style="background-color: #f5f6f7;">
                                ...
                            </p>
                        </div>
                    </div>
                </div>
            `
        responseContainer.innerHTML += thinking;
        responseContainer.scrollTo(0, responseContainer.scrollHeight);
    }, 600);

    setTimeout(() => {
        const thinking = document.getElementById('thinking');
        thinking.remove();
        generateRespone(answerHtml);
    }, 2000);
}

function generateRespone(answerHtml) {
    const responseContainer = document.getElementById('response-container');
    responseContainer.innerHTML += answerHtml;
    responseContainer.scrollTo(0, responseContainer.scrollHeight);
    attachEventListeners();  // Reattach event listeners to the new options
}

function attachEventListeners() {
    document.getElementById('about-us').onclick = function () {
        showMessage(document.getElementById('about-us').textContent, 'Luna là chuỗi khách sạn cao cấp với mong muốn đem lại cho bạn những phút giây thư giãn thật tuyệt vời và đáng nhớ.');
    };
    document.getElementById('best-seller').onclick = function () {
        showMessage(document.getElementById('best-seller').textContent, 'Hiện nay, Opera là loại phòng được đánh giá cao nhất của Luna với không gian rộng rãi và thoáng mát, hứa hẹn sẽ đem đến cho bạn những trải nghiệm tuyệt vời!');
    };
    document.getElementById('services').onclick = function () {
        showMessage(document.getElementById('services').textContent, 'Các dịch vụ hiện có của Luna bao gồm: Ăn uống, Gym, Party,... Bạn có thể vảo phần "Dịch vụ" để có thể xem chi tiết.');
    };
    document.getElementById('policy').onclick = function () {
        showMessage(document.getElementById('policy').textContent, 'Luna hỗ trợ hoàn trả 90% phí đặt phòng đã thanh toán trước vào ví tài khoản cá nhân của bạn.');
    };
    document.getElementById('contact-staff').onclick = function () {
        showMessage(document.getElementById('contact-staff').textContent, 'Bạn có thể liên hệ trực tiếp cho chúng tôi qua hotline 0123456789. Hoặc đăng nhập tài khoản của Luna để có thể nhắn tin trao đổi với nhân viên tư vấn.');
    }

    attachEventListeners();
}