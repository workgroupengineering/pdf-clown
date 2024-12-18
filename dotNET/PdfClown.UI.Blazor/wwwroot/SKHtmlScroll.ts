// alias for a potential skia html canvas
type SKHtmlScrollElement = {
    SKHtmlScroll: SKHtmlScroll
} & HTMLCanvasElement;
type MoveActionFunction = (a: number[]) => void;
type SizeActionFunction = (w: number, h: number) => void;

export class SKHtmlScroll {
    static elements: Map<string, SKHtmlScrollElement>;
    static observer: ResizeObserver;
    moveAction: MoveActionFunction;
    sizeAction: SizeActionFunction;

    public static init(elementId: string, moveAction: MoveActionFunction, sizeAction: SizeActionFunction) {
        if (!SKHtmlScroll.elements) {
            SKHtmlScroll.elements = new Map<string, SKHtmlScrollElement>();
            SKHtmlScroll.observer = new ResizeObserver((entries) => {
                for (let entry of entries) {
                    SKHtmlScroll.sizeAllocated(entry.target as SKHtmlScrollElement);
                }
            });
        }
        const element = document.getElementById(elementId);
        var scrollElement = element as SKHtmlScrollElement;
        if (!scrollElement) {
            console.error(`No canvas element was provided.`);
            return;
        }
        SKHtmlScroll.elements.set(elementId, scrollElement);
        const view = new SKHtmlScroll(scrollElement, moveAction, sizeAction);
        scrollElement.SKHtmlScroll = view;
    }

    public static getDPR(): number {
        return window.devicePixelRatio;
    }

    public static deinit(elementId: string) {
        const element = SKHtmlScroll.elements.get(elementId);
        SKHtmlScroll.elements.delete(elementId);
        element.SKHtmlScroll.deconstruct(element);
    }

    public static requestLock(elementId: string) {
        const element = SKHtmlScroll.elements.get(elementId);
        element.requestPointerLock();
    }

    public static setCapture(elementId: string, pointerId: number) {
        const element = SKHtmlScroll.elements.get(elementId);
        element.setPointerCapture(pointerId);
    }

    public static releaseCapture(elementId: string, pointerId: number) {
        const element = SKHtmlScroll.elements.get(elementId);
        element.releasePointerCapture(pointerId);
    }

    public static changeCursor(elementId: string, cursorName: string) {
        const element = SKHtmlScroll.elements.get(elementId);
        element.style.cursor = cursorName;
    }

    static sizeAllocated(element: SKHtmlScrollElement) {
        element.SKHtmlScroll.sizeAction(element.clientWidth, element.clientHeight);
    }

    public static unwrapp(jsObject: any): number[] {
        return jsObject as number[];
    }

    static eventArgsCreator(e: PointerEvent): any {
        return {
            "pointerId": e.pointerId,
            "button": SKHtmlScroll.getButton(e),
            "offsetX": e.offsetX,
            "offsetY": e.offsetY,
            "altKey": e.altKey,
            "ctrlKey": e.ctrlKey,
            "shiftKey": e.shiftKey,
            "metaKey": e.metaKey,
        };
    }

    private static getButton(e: PointerEvent): number {
        return e.buttons == 1 ? 0 : e.buttons == 2 ? 2 : e.buttons == 4 ? 1 : -1;
    }

    static getKeyModifiers(e: PointerEvent): number {
        var result = 0;
        if (e.altKey)
            result |= 1;
        if (e.ctrlKey)
            result |= 2;
        if (e.shiftKey)
            result |= 4;
        if (e.metaKey)
            result |= 8;
        return result;
    }

    public constructor(element: SKHtmlScrollElement, moveAction: any, sizeAction: any) {
        this.moveAction = moveAction;
        this.sizeAction = sizeAction;
        element.addEventListener('pointermove', this.OnPointerMove);
        SKHtmlScroll.observer.observe(element);
    }

    deconstruct(element: SKHtmlScrollElement) {
        element.removeEventListener('pointermove', this.OnPointerMove);
        SKHtmlScroll.observer.unobserve(element);
    }

    OnPointerMove = (e: PointerEvent) => {
        e.preventDefault();
        e.stopPropagation();
        this.moveAction([e.pointerId, SKHtmlScroll.getButton(e), e.offsetX, e.offsetY, SKHtmlScroll.getKeyModifiers(e)]);
    }

}
