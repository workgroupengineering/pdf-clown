﻿export class SKHtmlScroll {
    static elements: Map<string, HTMLElement>;
    static observer: ResizeObserver;
    htmlElement: HTMLElement;
    htmlElementId: string;
    moveAction: any;
    sizeAction: any;
    SKHtmlScroll: SKHtmlScroll;

    public static init(element: HTMLElement, elementId: string, moveAction: any, sizeAction: any) {
        if (!SKHtmlScroll.elements) {
            SKHtmlScroll.elements = new Map<string, HTMLElement>();
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

    public static initById(elementId: string, moveAction: any, sizeAction: any) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.init(element, elementId, moveAction, sizeAction);
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

    static changeCursor(element: HTMLElement, cursorName: string) {
        element.style.cursor = cursorName;
    }

    static changeCursorById(elementId: string, cursorName: string) {
        const element = document.getElementById(elementId);
        SKHtmlScroll.changeCursor(element, cursorName);
    }

    static sizeAllocated(element: Element) {
        element.SKHtmlScroll.sizeAction.invokeMethod("Invoke", element.clientWidth, element.clientHeight);
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

    public constructor(element: HTMLElement, elementId: string, moveAction: any, sizeAction: any) {
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

    OnPointerMove = (e: PointerEvent) => {
        e.preventDefault();
        e.stopPropagation();
        this.moveAction.invokeMethod("Invoke", SKHtmlScroll.eventArgsCreator(e));
    }

}
