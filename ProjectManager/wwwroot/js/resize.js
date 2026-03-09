let dotNetRef;

window.setDotNetRef = (ref) => {
    dotNetRef = ref;
};

window.addGlobalListeners = () => {
    document.addEventListener('mousemove', globalMouseMove);
    document.addEventListener('mouseup', globalMouseUp);
};

window.removeGlobalListeners = () => {
    document.removeEventListener('mousemove', globalMouseMove);
    document.removeEventListener('mouseup', globalMouseUp);
};

function globalMouseMove(e) {
    if (dotNetRef) {
        dotNetRef.invokeMethodAsync('OnGlobalMouseMove', e.clientX);
    }
}

function globalMouseUp(e) {
    if (dotNetRef) {
        dotNetRef.invokeMethodAsync('OnGlobalMouseUp');
    }
}
