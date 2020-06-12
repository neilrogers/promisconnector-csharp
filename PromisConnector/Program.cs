using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PromisConnector
{
    class Program
    {
        Promis promis = null;
        static void Main(string[] args)
        {
            Program program = new Program();
            program.promis = new Promis();

            program.promis.login(program.promis.config["username"].ToString(), program.promis.config["password"].ToString(), program.promis.config["entityid"].ToString());

            program.sendTaxCodes();
            program.sendContacts();
            dynamic invoice = program.sendInvoice();

            string invoicePdfPath = @"./invoice.pdf";
            if (System.Diagnostics.Debugger.IsAttached) invoicePdfPath = @"../../../invoice.pdf";
            program.promis.sendPDF(invoicePdfPath, invoice["response"]["data"]["Created"][0]["_id"].ToString());

            dynamic payments = program.promis.getPayments("?Version=Supplier&Status=PAID&Date=>2020-06-11");
            Console.WriteLine(payments.ToString());

        }


        dynamic sendTaxCodes()
        {
            string xml = @"<TaxCodes>
                <TaxCode>
                    <EntityID> " + this.promis.entityId + @" </EntityID>
                    <TaxCodeForeignID>GrossUp 0%</TaxCodeForeignID>
                    <TaxCodeName>GrossUp 0%</TaxCodeName>
                    <TaxCodePercentage>0</TaxCodePercentage>
                    <TaxCodeType>GrossUp</TaxCodeType>
                    <ClientPercentageDefault>true</ClientPercentageDefault>
                </TaxCode>
                <TaxCode>
                    <EntityID> " + this.promis.entityId + @" </EntityID>
                    <TaxCodeForeignID>AU-TaxClaimable 0%</TaxCodeForeignID>
                    <TaxCodeName>AU-TaxClaimable 0%</TaxCodeName>
                    <TaxCodePercentage>0</TaxCodePercentage>
                    <TaxCodeType>AU-TaxClaimable</TaxCodeType>
                    <ClientPercentageDefault>true</ClientPercentageDefault>
                </TaxCode>
                <TaxCode>
                    <EntityID>" + this.promis.entityId + @"</EntityID>
                    <TaxCodeForeignID>AU-TaxClaimable 10%</TaxCodeForeignID>
                    <TaxCodeName>AU-TaxClaimable 10%</TaxCodeName>
                    <TaxCodePercentage>0.1</TaxCodePercentage>
                    <TaxCodeType>AU-TaxClaimable</TaxCodeType>
                    <ClientPercentageDefault>true</ClientPercentageDefault>
                </TaxCode>
            </TaxCodes>";

            dynamic response = this.promis.postTaxCodes(xml);
            return response;
        }

        dynamic sendContacts()
        {
            string xml = @"<Contacts>
                <Contact>
                    <EntityID> " + this.promis.entityId + @" </EntityID>
                    <ContactForeignID>b40f61b3-b1ac-478f-909b-065b43f0efc9</ContactForeignID>
                    <CompanyName>PF11</CompanyName>
                    <ABN>12113628789</ABN>
                    <Emails>
                        <Email>pf11@promis.co</Email>
                    </Emails>
                    <Addresses>
                        <Address>
                            <Country>Australia</Country>
                            <State>QLD</State>
                            <PostCode>4567</PostCode>
                            <City>Noosa Heads</City>
                            <Street>PO Box 197</Street>
                            <Type>STREET</Type>
                        </Address>
                        <Address>
                            <Country>Australia</Country>
                            <State>QLD</State>
                            <PostCode>4567</PostCode>
                            <City>Noosa Heads</City>
                            <Street>PO Box 197</Street>
                            <Type>POST</Type>
                        </Address>
                    </Addresses>
                </Contact>
            </Contacts>";

            dynamic response = this.promis.postContacts(xml);
            return response;
        }

        dynamic sendInvoice() {
            string invoiceXmlPath = @"./invoice.xml";
            if (System.Diagnostics.Debugger.IsAttached) invoiceXmlPath = @"../../../invoice.xml";

            string invoicexml = File.ReadAllText(invoiceXmlPath);
            //make the invoice unique
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            invoicexml = invoicexml.Replace("<InvoiceForeignID>0f07dbd5-60e8-4539-8665-6df58ada8f2a</InvoiceForeignID>", "<InvoiceForeignID>IN-FOREIGN-" + unixTimestamp + "</InvoiceForeignID>");
            invoicexml = invoicexml.Replace("<InvoiceNumber>INV-0117</InvoiceNumber>", "<InvoiceNumber>INV-" + unixTimestamp + "</InvoiceNumber>");
            invoicexml = invoicexml.Replace("[ENTITYID]", this.promis.entityId);
            dynamic invoiceCreatedResponse = this.promis.postInvoice(invoicexml);
            return invoiceCreatedResponse;
        }


    }
}
