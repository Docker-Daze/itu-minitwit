/*
The script maintains scroll position on redirect
Co-authored by https://stackoverflow.com/a/62881829
*/

document.addEventListener("DOMContentLoaded", () => {
    const scrollData = JSON.parse(sessionStorage.getItem("scrollData"));
    if (scrollData && scrollData.url === window.location.href) { // If changing page it should scroll to the top
    window.scrollTo(0, parseInt(scrollData.scrollPosition));
}
});

window.addEventListener("beforeunload", () => {
    const scrollData = {
    url: window.location.href,
    scrollPosition: window.scrollY
};
    sessionStorage.setItem("scrollData", JSON.stringify(scrollData));
});
