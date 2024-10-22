export class SKHtmlScroll {

    static init(element, elementId, moveAction, sizeAction) {
        if (!SKHtmlScroll.elements) {
            SKHtmlScroll.elements = new Map();
            SKHtmlScroll.observer = new ResizeObserver((entries) => {
                for (let entry of entries) {
                    SKHtmlScroll.sizeAllocated(entry.target);
                }
            });
        }
        SKHtmlScroll.elements[elementId] = element;
        const view = new SKHtmlScroll(element, elementId, moveAction, sizeAction);
        element.SKHtmlScroll = view;        
    }

    static initById(elementId, moveAction, sizeAction) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.init(element, elementId, moveAction, sizeAction);
    }

    static deinit(elementId) {
        const element = SKHtmlScroll.elements[elementId];
        SKHtmlScroll.elements.delete(elementId);
        element.deinit();
    }    

    static requestLock(element) {
        element.requestPointerLock();
    }

    static requestLockById(elementId) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.requestLock(element);
    }

    static setCapture(element, pointerId) {
        element.setPointerCapture(pointerId);
    }

    static setCaptureById(elementId, pointerId) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.setCapture(element, pointerId);
    }

    static releaseCapture(element, pointerId) {
        element.releasePointerCapture(pointerId);
    }

    static releaseCaptureById(elementId, pointerId) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.releaseCapture(element, pointerId);
    }

    static changeCursor(element, cursorName) {
        element.style.cursor = cursorName;
    }

    static changeCursorById(elementId, cursorName) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.changeCursor(element, cursorName);
    }

    static sizeAllocated(element) {
        element.SKHtmlScroll.sizeAction.invokeMethod("Invoke", element.clientWidth, element.clientHeight);
    }

    static eventArgsCreator(event) {
        return {
            "pointerId": event.pointerId,
            "button": event.buttons == 1 ? 0 : event.buttons == 2 ? 2 : event.buttons == 4 ? 1 : -1,
            "offsetX": event.offsetX,
            "offsetY": event.offsetY,
            "altKey": event.altKey,
            "ctrlKey": event.ctrlKey,
            "shiftKey": event.shiftKey,
            "metaKey": event.metaKey,
        };
    }

    constructor(element, elementId, moveAction, sizeAction) {
        this.htmlElement = element;
        this.htmlElementId = elementId;
        this.moveAction = moveAction;
        this.sizeAction = sizeAction;
        this.htmlElement.addEventListener('pointermove', this.OnPointerMove);        
        SKHtmlScroll.observer.observe(this.htmlElement);
    }

    deinit() {
        this.htmlElement.removeEventListener('pointermove', this.OnPointerMove);     
        SKHtmlScroll.observer.unobserve(this.htmlElement);
    }

    OnPointerMove = (e) => {
        e.preventDefault();
        e.stopPropagation();
        this.moveAction.invokeMethod("Invoke", SKHtmlScroll.eventArgsCreator(e));
    }
}
