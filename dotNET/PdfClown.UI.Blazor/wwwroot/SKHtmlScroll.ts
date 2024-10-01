export class SKHtmlScroll {
    static elements: any;
    htmlElement: HTMLElement;
    htmlElementId: string;
    moveAction: any;
    SKHtmlScroll: SKHtmlScroll;

    public static init(element: HTMLElement, elementId: string, moveAction: any) {
        if (!SKHtmlScroll.elements)
            SKHtmlScroll.elements = new Map();
        SKHtmlScroll.elements[elementId] = element;
        const view = new SKHtmlScroll(element, elementId, moveAction);
        element.SKHtmlScroll = view;
    }

    public static initById(elementId: string, moveAction: any) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.init(element, elementId, moveAction);
    }

    public static requestLock(element: HTMLElement) {
        element.requestPointerLock();
    }

    public static requestLockById(elementId: string) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.requestLock(element);
    }

    public static setCapture(element: HTMLElement, pointerId: number) {
        element.setPointerCapture(pointerId);
    }

    public static setCaptureById(elementId: string, pointerId: number) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.setCapture(element, pointerId);
    }

    public static releaseCapture(element: HTMLElement, pointerId: number) {
        element.releasePointerCapture(pointerId);
    }

    public static releaseCaptureById(elementId: string, pointerId: number) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.releaseCapture(element, pointerId);
    }

    public constructor(element: HTMLElement, elementId: string, moveAction: any) {
        this.htmlElement = element;
        this.htmlElementId = elementId;
        this.moveAction = moveAction;
        element.addEventListener('pointermove', this.OnPointerMove);
    }

    static eventArgsCreator(e: PointerEvent): object {
        return {
            "pointerId": e.pointerId,
            "button": e.buttons == 1 ? 0 : e.buttons == 2 ? 2 : e.buttons == 4 ? 1 : -1,
            "offsetX": e.offsetX,
            "offsetY": e.offsetY,
            "altKey": e.altKey,
            "ctrlKey": e.ctrlKey,
            "shiftKey": e.shiftKey,
            "metaKey": e.metaKey,
        };
    }

    deinit() {
        this.htmlElement.removeEventListener('pointermove', this.OnPointerMove);
    }

    OnPointerMove = (e: PointerEvent) => {
        e.preventDefault();
        this.moveAction.invokeMethod("Invoke", SKHtmlScroll.eventArgsCreator(e));
    }

}
