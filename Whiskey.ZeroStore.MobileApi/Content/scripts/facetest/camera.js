var mediaStreamTrack, canvas, context, video, snap, close;
function openCamera(type) {
    function $(elem) {
        return document.querySelector(elem);
    }
    // 获取媒体方法（旧方法）
    navigator.getMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMeddia || navigator.msGetUserMedia;
    if (type == 1) {

        canvas = $('#canvas');
        context = canvas.getContext('2d');
        video = $('#video');
        snap = $('#snap');
        close = $('#close');
    } else if (type == 2) {
        canvas = $('.canvas_2');
        context = canvas.getContext('2d');
        video = $('#video2');
        snap = $('#snap2');
    }
    // 获取媒体方法（新方法）
    // 使用新方法打开摄像头
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia({
            video: true,
            audio: false
        }).then(function (stream) {
            mediaStreamTrack = typeof stream.stop === 'function' ? stream : stream.getTracks()[0];
            video.src = (window.URL || window.webkitURL).createObjectURL(stream);
            video.play();
            context.clearRect(0, 0, canvas.width, canvas.height);
        }).catch(function (err) {
            console.log(err);
        })
    }
        // 使用旧方法打开摄像头
    else if (navigator.getMedia) {
        navigator.getMedia({
            video: true
        }, function (stream) {
            mediaStreamTrack = stream.getTracks()[0];

            video.src = (window.URL || window.webkitURL).createObjectURL(stream);
            video.play();
        }, function (err) {
            console.log(err);
        });
    }

    // 截取图像
    snap.onclick= function () {
        if (type == 1) {
            context.drawImage(video, 0, 0, video.width, video.height);
        } else {
            context.drawImage(video, 0, 70, 352, 266);
            Camera.serach();
        }
    };

    //close.addEventListener('click', function() {
    // mediaStreamTrack && mediaStreamTrack.stop();
    //}, false);

    // 上传截取的图像
    //upload.addEventListener('click', function() {
    //  jQuery.post('/uploadSnap.php', {
    //    snapData: canvas.toDataURL('image/png')
    //  }).done(function(rs) {
    //    rs = JSON.parse(rs);
    // 
    //    console.log(rs);
    // 
    //    uploaded.src = rs.path;
    //  }).fail(function(err) {
    //    console.log(err);
    //  });
    //}, false);

}