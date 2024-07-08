window.registerStateClickEvents = function (dotNetHelper) {
    console.log("Start State click register");
    const svgElement = document.querySelector('svg');
    if (svgElement) {
        const paths = svgElement.querySelectorAll('path');
        paths.forEach(path => {
            console.log("State click register: Found path");
            path.addEventListener('click', function (event) {
                const stateId = path.getAttribute('id');
                if (stateId) {
                    console.log(stateId);
                    dotNetHelper.invokeMethodAsync('LogStateId', stateId);
                }
            });
        });
    }
};