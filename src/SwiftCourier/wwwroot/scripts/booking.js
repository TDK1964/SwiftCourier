﻿$(document).ready(function () {
    var GCT_RATE = 0.165;

    $('.checkbo').checkBo();

    if ($('#PickupRequired').is(':checked'))
    {
        $('#PickupAddress').parents('div[class="form-group"]').show();
        $('#PickupContactNumber').parents('div[class="form-group"]').show();
    } else {
        $('#PickupAddress').parents('div[class="form-group"]').hide();
        $('#PickupContactNumber').parents('div[class="form-group"]').hide();
    }

    $('#PickupRequired').on('change', function () {
        if ($(this).is(':checked'))
        {
            $('#PickupAddress').parents('div[class="form-group"]').show();
            $('#PickupContactNumber').parents('div[class="form-group"]').show();
        } else {
            $('#PickupAddress').parents('div[class="form-group"]').hide();
            $('#PickupContactNumber').parents('div[class="form-group"]').hide();
        }
    });

    $('#CustomerId').on('change', function (e) {
        var customerId = $(this).val();
        var pickupRequired = $('#PickupRequired').val();

        if (customerId) {
            $.ajax({
                cache: false,
                dataType: 'json',
                type: 'GET',
                url: '/api/customer/' + customerId,
                success: function (data) {
                    if (data) {
                        if (pickupRequired) {
                            $('#PickupAddress').val(data.Address);
                            $('#PickupContactNumber').val(data.ContactNumber);
                        } else {
                            $('#PickupAddress').val('');
                            $('#PickupContactNumber').val('');
                        }

                        if (data.TaxExempted) {
                            GCT_RATE = 0.00;
                        } else {
                            GCT_RATE = 0.165;
                        }
                    } else {
                        GCT_RATE = 0.165;
                    }
                },
                error: function () {
                    GCT_RATE = 0.165;
                },
                complete: function () {
                    console.log(GCT_RATE);
                    $('#ServiceId').trigger('change');
                }
            });
        }
    });

    $('#ServiceId').on('change', function (e) {
        var serviceId = $(this).val();
        if (serviceId) {
            $.ajax({
                cache: false,
                dataType: 'json',
                type: 'GET',
                url: '/api/service/' + serviceId,
                success: function (data) {
                    if (data) {
                        $('#Invoice_ServiceCost').val(data.Cost);
                        var gct = data.Cost * GCT_RATE;
                        $('#Invoice_GCT').val(gct);
                        $('#Invoice_Total').val(data.Cost + gct);
                    } else {
                        $('#Invoice_ServiceCost').val('');
                        $('#Invoice_GCT').val('');
                        $('#Invoice_Total').val('');
                    }
                },
                error: function () {
                    $('#Invoice_ServiceCost').val('');
                    $('#Invoice_GCT').val('');
                    $('#Invoice_Total').val('');
                }
            });
        }
    });
});