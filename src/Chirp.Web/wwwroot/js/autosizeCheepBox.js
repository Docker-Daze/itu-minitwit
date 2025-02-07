document.addEventListener("DOMContentLoaded", function () {
    const cheepBox = document.getElementById("Message");
    const charCount = document.getElementById("charCount");
    const maxLength = 160;

    if (cheepBox && charCount) {
        cheepBox.addEventListener("input", function () {
            this.style.height = "auto";
            this.style.height = `${this.scrollHeight}px`;
            
            const charsLeft = maxLength - this.value.length;
            charCount.textContent = `Characters left: ${charsLeft}`;
        });
    }
});