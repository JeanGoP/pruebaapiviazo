<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:template match="/">
		<html>
			<body style="font-family: Arial, sans-serif; color: #333;">
				<h2>Mensaje de tu Gurú</h2>

				<p>
					Hola <strong>
						<xsl:value-of select="/top/DataInfo/@NombreCliente"/>
					</strong>,
				</p>

				<p>
					Tu gurú <strong>
						<xsl:value-of select="/top/DataInfo/@NombreGuru"/>
					</strong> te ha enviado un mensaje respecto a tu reserva <strong>
						#<xsl:value-of select="/top/DataInfo/@IdReserva"/>
					</strong>:
				</p>

				<p style="background-color: #f9f9f9; padding: 15px; border-left: 5px solid #4CAF50; margin-top: 10px; border-radius: 5px;">
					<em>
						<!-- Usamos xsl:copy-of para copiar el contenido HTML sin alterarlo -->
						<xsl:copy-of select="/top/DataInfo/MensajeGuru/node()"/>
					</em>
				</p>

				<p>
					Si deseas responder o necesitas más información, puedes contactarnos escribiendo a <a href="mailto:{/top/DataInfo/@Email}">
						<xsl:value-of select="/top/DataInfo/@Email"/>
					</a>.
				</p>

				<p style="margin-top: 20px;">
					¡Gracias por confiar en nosotros!<br/>Equipo de Atención al Cliente
				</p>
			</body>
		</html>
	</xsl:template>

</xsl:stylesheet>
