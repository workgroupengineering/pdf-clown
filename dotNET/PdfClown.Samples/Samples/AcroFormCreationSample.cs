using PdfClown.Documents;
using PdfClown.Documents.Contents.Composition;
using PdfClown.Documents.Contents.Entities;
using PdfClown.Documents.Contents.Fonts;
using PdfClown.Documents.Contents.XObjects;
using PdfClown.Documents.Interaction.Actions;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Documents.Interaction.Forms;
using PdfClown.Documents.Interaction.Forms.Styles;
using PdfClown.Files;

using System;
using System.Collections.Generic;
using SkiaSharp;

namespace PdfClown.Samples.CLI
{
    /**
      <summary>This sample demonstrates how to insert AcroForm fields into a PDF document.</summary>
    */
    public class AcroFormCreationSample : Sample
    {
        public override void Run()
        {
            // 1. PDF file instantiation.
            File file = new File();
            Document document = file.Document;

            // 2. Content creation.
            Populate(document);

            // 3. Serialize the PDF file!
            Serialize(file, "AcroForm", "inserting AcroForm fields", "Acroform, creation, annotations, actions, javascript, button, combo, textbox, radio button");
        }

        private void Populate(Document document)
        {
            /*
              NOTE: In order to insert a field into a document, you have to follow these steps:
              1. Define the form fields collection that will gather your fields (NOTE: the form field collection is global to the document);
              2. Define the pages where to place the fields;
              3. Define the appearance style to render your fields;
              4. Create each field of yours:
                4.1. instantiate your field into the page;
                4.2. apply the appearance style to your field;
                4.3. insert your field into the fields collection.
            */

            // 1. Define the form fields collection!
            Form form = document.Form;
            Fields fields = form.Fields;

            // 2. Define the page where to place the fields!
            var page = new Page(document);
            document.Pages.Add(page);

            // 3. Define the appearance style to apply to the fields!
            var fieldStyle = new DefaultStyle();
            fieldStyle.FontSize = 12;
            fieldStyle.GraphicsVisibile = true;

            var composer = new PrimitiveComposer(page);
            composer.SetFont(FontType1.Load(document, FontType1.FamilyEnum.Courier, true, false), 14);

            // 4. Field creation.
            // 4.a. Push button.
            {
                composer.ShowText(
                  "PushButton:",
                  new SKPoint(140, 68),
                  XAlignmentEnum.Right,
                  YAlignmentEnum.Middle,
                  0);

                var fieldWidget = new Widget(
                  page,
                  SKRect.Create(150, 50, 136, 36));
                fieldWidget.Actions.OnActivate = new JavaScript(
                  document,
                  "app.alert(\"Radio button currently selected: '\" + this.getField(\"myRadio\").value + \"'.\",3,0,\"Activation event\");");
                var field = new PushButton(
                  "okButton",
                  fieldWidget,
                  "Push" // Current value.
                  ); // 4.1. Field instantiation.
                fields.Add(field); // 4.2. Field insertion into the fields collection.
                fieldStyle.Apply(field); // 4.3. Appearance style applied.

                {
                    BlockComposer blockComposer = new BlockComposer(composer);
                    blockComposer.Begin(SKRect.Create(296, 50, page.Size.Width - 336, 36), XAlignmentEnum.Left, YAlignmentEnum.Middle);
                    composer.SetFont(composer.State.Font, 7);
                    blockComposer.ShowText("If you click this push button, a javascript action should prompt you an alert box responding to the activation event triggered by your PDF viewer.");
                    blockComposer.End();
                }
            }

            // 4.b. Check box.
            {
                composer.ShowText("CheckBox:", new SKPoint(140, 118), XAlignmentEnum.Right, YAlignmentEnum.Middle, 0);
                var field = new CheckBox("myCheck", new Widget(page, SKRect.Create(150, 100, 36, 36)), true); // 4.1. Field instantiation.
                fieldStyle.Apply(field);
                fields.Add(field);
                field = new CheckBox("myCheck2", new Widget(page, SKRect.Create(200, 100, 36, 36)), true); // 4.1. Field instantiation.
                fieldStyle.Apply(field);
                fields.Add(field);
                field = new CheckBox("myCheck3", new Widget(page, SKRect.Create(250, 100, 36, 36)), false); // 4.1. Field instantiation.
                fields.Add(field); // 4.2. Field insertion into the fields collection.
                fieldStyle.Apply(field); // 4.3. Appearance style applied.
            }

            // 4.c. Radio button.
            {
                composer.ShowText("RadioButton:", new SKPoint(140, 168), XAlignmentEnum.Right, YAlignmentEnum.Middle, 0);
                var field = new RadioButton(
                  "myRadio",
                  /*
                    NOTE: A radio button field typically combines multiple alternative widgets.
                  */
                  new Widget[]
                  {
                      new Widget( page, SKRect.Create(150, 150, 36, 36), "first" ),
                      new Widget( page, SKRect.Create(200, 150, 36, 36), "second" ),
                      new Widget( page, SKRect.Create(250, 150, 36, 36), "third" )
                  },
                  "second" // Selected item (it MUST correspond to one of the available widgets' names).
                  ); // 4.1. Field instantiation.
                fields.Add(field); // 4.2. Field insertion into the fields collection.
                fieldStyle.Apply(field); // 4.3. Appearance style applied.
            }

            // 4.d. Text field.
            {
                composer.ShowText("TextField:", new SKPoint(140, 218), XAlignmentEnum.Right, YAlignmentEnum.Middle, 0);
                var field = new TextField("myText", new Widget(page, SKRect.Create(150, 200, 200, 36)), "Carmen Consoli"); // 4.1. Field instantiation. // Current value.

                field.SpellChecked = false; // Avoids text spell check.
                FieldActions fieldActions = new FieldActions(document);
                field.Actions = fieldActions;
                fieldActions.OnValidate = new JavaScript(
                  document,
                  "app.alert(\"Text '\" + this.getField(\"myText\").value + \"' has changed!\",3,0,\"Validation event\");"
                  );
                fields.Add(field); // 4.2. Field insertion into the fields collection.
                fieldStyle.Apply(field); // 4.3. Appearance style applied.

                {
                    BlockComposer blockComposer = new BlockComposer(composer);
                    blockComposer.Begin(SKRect.Create(360, 200, page.Size.Width - 400, 36), XAlignmentEnum.Left, YAlignmentEnum.Middle);
                    composer.SetFont(composer.State.Font, 7);
                    blockComposer.ShowText("If you leave this text field after changing its content, a javascript action should prompt you an alert box responding to the validation event triggered by your PDF viewer.");
                    blockComposer.End();
                }
            }

            // 4.e. Choice fields.
            {
                // Preparing the item list that we'll use for choice fields (a list box and a combo box (see below))...
                ChoiceItems items = new ChoiceItems(document);
                items.Add("Tori Amos");
                items.Add("Anouk");
                items.Add("Joan Baez");
                items.Add("Rachele Bastreghi");
                items.Add("Anna Calvi");
                items.Add("Tracy Chapman");
                items.Add("Carmen Consoli");
                items.Add("Ani DiFranco");
                items.Add("Cristina Dona'");
                items.Add("Nathalie Giannitrapani");
                items.Add("PJ Harvey");
                items.Add("Billie Holiday");
                items.Add("Joan As Police Woman");
                items.Add("Joan Jett");
                items.Add("Janis Joplin");
                items.Add("Angelique Kidjo");
                items.Add("Patrizia Laquidara");
                items.Add("Annie Lennox");
                items.Add("Loreena McKennitt");
                items.Add("Joni Mitchell");
                items.Add("Alanis Morissette");
                items.Add("Yael Naim");
                items.Add("Noa");
                items.Add("Sinead O'Connor");
                items.Add("Dolores O'Riordan");
                items.Add("Nina Persson");
                items.Add("Brisa Roche'");
                items.Add("Roberta Sammarelli");
                items.Add("Cristina Scabbia");
                items.Add("Nina Simone");
                items.Add("Skin");
                items.Add("Patti Smith");
                items.Add("Fatima Spar");
                items.Add("Thony (F.V.Caiozzo)");
                items.Add("Paola Turci");
                items.Add("Sarah Vaughan");
                items.Add("Nina Zilli");

                // 4.e1. List box.
                {
                    composer.ShowText("ListBox:", new SKPoint(140, 268), XAlignmentEnum.Right, YAlignmentEnum.Middle, 0);
                    ListBox field = new ListBox("myList", new Widget(page, SKRect.Create(150, 250, 200, 70))); // 4.1. Field instantiation.
                    field.Items = items; // List items assignment.
                    field.MultiSelect = false; // Multiple items may not be selected simultaneously.
                    field.Value = "Carmen Consoli"; // Selected item.
                    fields.Add(field); // 4.2. Field insertion into the fields collection.
                    fieldStyle.Apply(field); // 4.3. Appearance style applied.
                }

                // 4.e2. Combo box.
                {
                    composer.ShowText("ComboBox:", new SKPoint(140, 350), XAlignmentEnum.Right, YAlignmentEnum.Middle, 0);
                    ComboBox field = new ComboBox("myCombo", new Widget(page, SKRect.Create(150, 334, 200, 36))); // 4.1. Field instantiation.
                    field.Items = items; // Combo items assignment.
                    field.Editable = true; // Text may be edited.
                    field.SpellChecked = false; // Avoids text spell check.
                    field.Value = "Carmen Consoli"; // Selected item.
                    fields.Add(field); // 4.2. Field insertion into the fields collection.
                    fieldStyle.Apply(field); // 4.3. Appearance style applied.
                }
            }

            composer.Flush();
        }
    }
}