(function () {
    'use strict';

    let socket;
    let canvas;

    const WS_URL = "ws://localhost:5000";

    const images = [];

    let timer = 0;

    function init() {

        let element = document.getElementById("canvas");

        element.width = document.body.clientWidth;
        element.height = document.body.clientHeight;

        canvas = element.getContext("2d");
        canvas.font = "10px Arial";

        let img;
        img = new Image();
        img.src = 'img/tubarao.png'
        images[0] = img;

        img = new Image();
        img.src = 'img/foca.png'
        images[1] = img;

        img = new Image();
        img.src = 'img/peixe.png'
        images[2] = img;

        img = new Image();
        img.src = 'img/alga.png'
        images[3] = img;

        connect();
    }

    function connect() {

        socket = new WebSocket(WS_URL + '?width=' + document.body.clientWidth + '&height=' + document.body.clientHeight);
        socket.onmessage = message;

        setInterval(function() {
            $('#timer').html((timer++)+'s');
        }, 1000);
    }

    function message(e) {
        const animals = JSON.parse(e.data);

        let element = document.getElementById("canvas");
        canvas.clearRect(0, 0, element.width, element.height);

        $('#total').html(animals.length);

        animals.forEach(e => {
            desenha(e);
        });
    }

    function desenha(e) {
        canvas.drawImage(images[e.specie], e.x, e.y, 100, 50);
        canvas.fillText('CALORIAS: '+e.calorias, e.x, e.y);
    }

    function add(animal) {
        socket.send(JSON.stringify({
            animal: animal,
            calorias: parseInt($('#calorias').val())
        }));
    }

    $(document).ready(function () {

        $('.add').on("click", (e) => {
            const animal = $(e.target).attr('type');
            add(animal);
        });

        init();
    });

})();


