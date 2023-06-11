// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


window.onload = () => {
    if (location.href.indexOf('student') !== -1) {
        console.log("Well Done!!!")
    }
}

// Set The Preview Image During Image Update Request

const fileInput = document.querySelector('.new-img');
const previewImage = document.getElementById('previewImg');

fileInput.addEventListener('change', function (event) {
    const file = event.target.files[0];

    if (file) {
        const reader = new FileReader();

        reader.addEventListener('load', function () {
            previewImage.setAttribute('src', reader.result);
        });

        reader.readAsDataURL(file);
    }
});