let observer = null;

// Watches the marker; calls OnReachedBottom (C#) when it becomes visible
export function observe(element, dotNetRef) {
    if (observer) observer.disconnect();

    observer = new IntersectionObserver(entries => {
        if (entries[0].isIntersecting) {
            dotNetRef.invokeMethodAsync("OnReachedBottom");
        }
    });
    observer.observe(element);
}

export function disconnect() {
    if (observer) observer.disconnect();
}