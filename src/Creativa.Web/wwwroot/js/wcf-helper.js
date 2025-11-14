// WCF SOAP Helper for Kendo DataSource
var WcfHelper = (function () {
    var serviceUrl = 'http://localhost:5084/NorthwindService/basichttp';

    function callWcfMethod(methodName, parameters) {
        var soapEnvelope = buildSoapEnvelope(methodName, parameters);

        return $.ajax({
            type: 'POST',
            url: serviceUrl,
            contentType: 'text/xml; charset=utf-8',
            dataType: 'xml',
            data: soapEnvelope,
            processData: false,
            beforeSend: function (xhr) {
                xhr.setRequestHeader('SOAPAction', 'http://tempuri.org/INorthwindService/' + methodName);
            }
        });
    }

    function buildSoapEnvelope(methodName, parameters) {
        var ns = "http://tempuri.org/";
        var parameterXml = Object.keys(parameters || {}).map(function (key) {
            var value = parameters[key] ?? "";
            return "<tem:" + key + ">" + escapeXml(value) + "</tem:" + key + ">";
        }).join("");

        return [
            '<?xml version="1.0" encoding="utf-8"?>',
            '<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="' + ns + '">',
            '<soap:Body>',
            '<tem:' + methodName + '>',
            parameterXml,
            '</tem:' + methodName + '>',
            '</soap:Body>',
            '</soap:Envelope>'
        ].join("");
    }

    function escapeXml(unsafe) {
        return unsafe.replace(/[<>&'"]/g, function (c) {
            switch (c) {
                case '<': return '&lt;';
                case '>': return '&gt;';
                case '&': return '&amp;';
                case '\'': return '&apos;';
                case '"': return '&quot;';
            }
        });
    }

    function parseCustomersResponse(xmlDoc) {
        console.log('XML Response:', xmlDoc);
        var customers = [];

        // Buscar elementos que terminen con ":Customer" o sean "Customer" (sin namespace)
        var customerElements = $(xmlDoc).find('*').filter(function () {
            return this.nodeName.indexOf('Customer') !== -1 &&
                this.nodeName !== 'GetCustomersByCountryResult';
        });

        customerElements.each(function () {
            var $customer = $(this);

            // Buscar hijos sin importar namespace
            function findChildText(parentElement, localName) {
                var child = $(parentElement).children().filter(function () {
                    return this.nodeName.indexOf(localName) !== -1;
                });
                return child.length > 0 ? child.text() : '';
            }

            var customer = {
                customerID: findChildText(this, 'CustomerID'),
                companyName: findChildText(this, 'CompanyName'),
                contactName: findChildText(this, 'ContactName'),
                phone: findChildText(this, 'Phone'),
                fax: findChildText(this, 'Fax')
            };

            customers.push(customer);
        });

        console.log('Parsed customers:', customers);
        return customers;
    }

    function parseOrdersResponse(xmlDoc) {
        console.log('XML Response (Orders):', xmlDoc);
        var orders = [];

        // Looks for the Result element and obtainas its direct children
        var resultElement = $(xmlDoc).find('*').filter(function () {
            return this.nodeName.indexOf('GetOrdersByCustomerResult') !== -1;
        }).first();

        // Obtain direct children that are Order elements
        var orderElements = $(resultElement).children().filter(function () {
            return this.nodeName.indexOf('Order') !== -1;
        });

        orderElements.each(function () {
            var $order = $(this);

            function findChildText(parentElement, localName) {
                var child = $(parentElement).children().filter(function () {
                    return this.nodeName.indexOf(localName) !== -1;
                });
                return child.length > 0 ? child.text() : '';
            }

            var order = {
                orderID: parseInt(findChildText(this, 'OrderID')) || 0,
                customerID: findChildText(this, 'CustomerID'),
                orderDate: findChildText(this, 'OrderDate'),
                shippedDate: findChildText(this, 'ShippedDate')
            };

            orders.push(order);
        });

        console.log('Parsed orders:', orders);
        return orders;
    }

    return {
        getCustomersByCountry: function (country) {
            return callWcfMethod('GetCustomersByCountry', { country: country })
                .then(function (xmlDoc) {
                    return parseCustomersResponse(xmlDoc);
                });
        },

        getOrdersByCustomer: function (customerId) {
            return callWcfMethod('GetOrdersByCustomer', { customerId: customerId })
                .then(function (xmlDoc) {
                    return parseOrdersResponse(xmlDoc);
                });
        }
    };
})();