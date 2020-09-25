<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>

  <xsl:key name="first_letter_term1" match="//IDXTERM" use="substring(translate(TERM1, 'ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ', 'abcdefghijklmnopqrstuvwxyzæøå'),1,1)" />
  <xsl:key name="ixdterm_by_term1" match="//IDXTERM" use="TERM1" />
  <xsl:key name="ixdterm_by_term12" match="//IDXTERM" use="concat(TERM1,'/',TERM2)" />
  
  <xsl:template match="/">
    <root>
      <content>
        <xsl:call-template  name="GetTop" />
      </content>
      <index>
    <xsl:for-each select="//IDXTERM[count(. | key('first_letter_term1', substring(translate(TERM1, 'ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ', 'abcdefghijklmnopqrstuvwxyzæøå'),1,1))[1]) = 1]">
      <xsl:sort select="substring(translate(TERM1, 'ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ', 'abcdefghijklmnopqrstuvwxyzæøå'),1,1)" />
      
      <indexitem>
        <xsl:variable name="firstletter">
          <xsl:value-of select="substring(translate(TERM1, 'abcdefghijklmnopqrstuvwxyzæøå','ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ' ),1,1)" />
        </xsl:variable>

        <xsl:attribute name="text">
          <xsl:value-of select="$firstletter" />          
        </xsl:attribute>

        <xsl:for-each select="//IDXTERM[substring(translate(TERM1, 'abcdefghijklmnopqrstuvwxyzæøå','ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ'),1,1)=$firstletter][count(. | key('ixdterm_by_term1', TERM1)[1]) = 1]">
          <xsl:sort select="TERM1" />
          
          <item>

            <xsl:attribute name="text">
              <xsl:value-of select="TERM1" />
            </xsl:attribute>

            <xsl:variable name="term1">
              <xsl:value-of select="TERM1" />
            </xsl:variable>

            <xsl:for-each select="//IDXTERM[TERM1=$term1][not (TERM2)]">
              <xsl:call-template name="FindParents" />
            </xsl:for-each>

            <xsl:for-each select="//IDXTERM[TERM1=$term1][TERM2!=''][count(. | key('ixdterm_by_term12', concat($term1,'/', TERM2))[1]) = 1]">
                <xsl:sort select="TERM2" />
                <item>
                  <xsl:attribute name="text">
                    <xsl:value-of select="TERM2" />
                  </xsl:attribute>

                  <xsl:variable name="term2">
                    <xsl:value-of select="TERM2" />
                  </xsl:variable>
                  <xsl:for-each select="//IDXTERM[TERM1=$term1 and TERM2=$term2]">
                    <xsl:call-template name="FindParents" />
                  </xsl:for-each>
                </item>
              </xsl:for-each>
              
          </item>
        </xsl:for-each>
      </indexitem>
    </xsl:for-each>
      </index>
    </root>
  </xsl:template>
  
  <xsl:template name="FindParents">
    <xsl:if test="ancestor::SEK[1]/@ID | ancestor::INNLEDNING[1]/@uid | ancestor::FORORD[1]/@uid">
      <link>
        <xsl:attribute name="id">
          <xsl:value-of select="@uid" />
        </xsl:attribute>
        <xsl:attribute name="target">
          <xsl:value-of select="ancestor::SEK[1]/@ID | ancestor::INNLEDNING[1]/@uid | ancestor::FORORD[1]/@uid"/>
        </xsl:attribute>
        <xsl:attribute name="pid">
          <xsl:choose>
            <xsl:when test="ancestor::SUBSEK5[1]/@ID">
              <xsl:value-of select="ancestor::SUBSEK5[1]/@ID"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="ancestor::SUBSEK4[1]/@ID">
                  <xsl:value-of select="ancestor::SUBSEK4[1]/@ID"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <xsl:when test="ancestor::SUBSEK3[1]/@ID">
                      <xsl:value-of select="ancestor::SUBSEK3[1]/@ID"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:choose>
                        <xsl:when test="ancestor::SUBSEK2[1]/@ID">
                          <xsl:value-of select="ancestor::SUBSEK2[1]/@ID"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:choose>
                            <xsl:when test="ancestor::SUBSEK1[1]/@ID">
                              <xsl:value-of select="ancestor::SUBSEK1[1]/@ID"/>
                            </xsl:when>
                            <xsl:otherwise>
                              <xsl:choose>
                                <xsl:when test="ancestor::SEK[1]/@ID">
                                  <xsl:value-of select="ancestor::SEK[1]/@ID"/>
                                </xsl:when>
                                <xsl:otherwise>
                                  <xsl:choose>
                                    <xsl:when test="ancestor::INNLEDNING[1]/@uid">
                                      <xsl:value-of select="ancestor::INNLEDNING[1]/@uid"/>
                                    </xsl:when>
                                    <xsl:otherwise>
                                      <xsl:choose>
                                        <xsl:when test="ancestor::FORORD[1]/@uid">
                                          <xsl:value-of select="ancestor::INNLEDNING[1]/@uid"/>
                                        </xsl:when>
                                      </xsl:choose>
                                    </xsl:otherwise>
                                  </xsl:choose>
                                </xsl:otherwise>
                              </xsl:choose>
                            </xsl:otherwise>
                          </xsl:choose>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </link>
    </xsl:if>
  </xsl:template>

  <xsl:template name="GetTop">
    <xsl:for-each select="//MVA/INNLDEL">
      <item>
        <xsl:attribute name="text">
          <xsl:text>Innledning</xsl:text>
        </xsl:attribute>
      <xsl:for-each select="INNLEDNING | FORORD | GLOSSAR">
        <item>
          <xsl:attribute name="id">
            <xsl:value-of select="@uid"/>
          </xsl:attribute>

          <xsl:attribute name="text">
            <xsl:for-each select="TIT/T">
              <xsl:value-of select="."/>
            </xsl:for-each>
          </xsl:attribute>
        </item>
        </xsl:for-each>
      </item>
      </xsl:for-each>

    <xsl:for-each select="MVA/HOVEDDEL">
      <xsl:for-each select="KAPITTEL">
        <item>
          <xsl:attribute name="text">
            <xsl:for-each select="TITTEL/T">
              <xsl:value-of select="."/>
            </xsl:for-each>
          </xsl:attribute>
          <xsl:call-template name="GetIndexItems" />
        </item>
      </xsl:for-each>
    </xsl:for-each>

  </xsl:template>


  <xsl:template name="GetIndexItems">
    <xsl:for-each select="UKAPITTEL | SEK | SUBSEK1 | SUBSEK2 | SUBSEK3 | SUBSEK4 | SUBSEK5">
    <xsl:choose>
      <xsl:when test="local-name() = 'UKAPITTEL'">
          <item>
            <xsl:attribute name="id">
              <xsl:value-of select="@uid"/>
            </xsl:attribute>

            
            <xsl:attribute name="text">
              <xsl:for-each select="UTITTEL/T">
                <xsl:value-of select="."/>
              </xsl:for-each>
            </xsl:attribute>
            <xsl:call-template name="GetIndexItems" />
          </item>
      </xsl:when>

      <xsl:when test="local-name() = 'SEK'">
        <item>
          <xsl:attribute name="id">
            <xsl:value-of select="@uid"/>
          </xsl:attribute>

          <xsl:attribute name="pid">
            <xsl:value-of select="@uid"/>
          </xsl:attribute>

          <xsl:attribute name="text">
            <xsl:for-each select="MS/T">
              <xsl:value-of select="."/>
            </xsl:for-each>
          </xsl:attribute>
          <xsl:call-template name="GetIndexItems" />
        </item>
      </xsl:when>

      <xsl:when test="local-name() = 'SUBSEK1'">
        <item>
          <xsl:attribute name="id">
            <xsl:value-of select="@uid"/>
          </xsl:attribute>

          <xsl:attribute name="pid">
            <xsl:value-of select="ancestor::SEK[1]/@uid" />
          </xsl:attribute>

          <xsl:attribute name="text">
            <xsl:for-each select="MS1/T">
              <xsl:value-of select="."/>
            </xsl:for-each>
          </xsl:attribute>
          <xsl:call-template name="GetIndexItems" />
        </item>
      </xsl:when>

      <xsl:when test="local-name() = 'SUBSEK2'">
        <item>
          <xsl:attribute name="id">
            <xsl:value-of select="@uid"/>
          </xsl:attribute>

          <xsl:attribute name="pid">
            <xsl:value-of select="ancestor::SEK[1]/@uid" />
          </xsl:attribute>

          <xsl:attribute name="text">
            <xsl:for-each select="MS2/T">
              <xsl:value-of select="."/>
            </xsl:for-each>
          </xsl:attribute>
          <xsl:call-template name="GetIndexItems" />
        </item>
      </xsl:when>

      <xsl:when test="local-name() = 'SUBSEK3'">
        <item>
          <xsl:attribute name="id">
            <xsl:value-of select="@uid"/>
          </xsl:attribute>

          <xsl:attribute name="pid">
            <xsl:value-of select="ancestor::SEK[1]/@uid" />
          </xsl:attribute>

          <xsl:attribute name="text">
            <xsl:for-each select="MS3/T">
              <xsl:value-of select="."/>
            </xsl:for-each>
          </xsl:attribute>
          <xsl:call-template name="GetIndexItems" />
        </item>
      </xsl:when>

      <xsl:when test="local-name() = 'SUBSEK4'">
        <item>
          <xsl:attribute name="id">
            <xsl:value-of select="ancestor::SEK[1]/@uid" />
          </xsl:attribute>

          <xsl:attribute name="pid">
            <xsl:call-template name="FindParents" />
          </xsl:attribute>

          <xsl:attribute name="text">
            <xsl:for-each select="MS4/T">
              <xsl:value-of select="."/>
            </xsl:for-each>
          </xsl:attribute>
          <xsl:call-template name="GetIndexItems" />
        </item>
      </xsl:when>

      <xsl:when test="local-name() = 'SUBSEK5'">
        <item>
          <xsl:attribute name="id">
            <xsl:value-of select="@uid"/>
          </xsl:attribute>

          <xsl:attribute name="pid">
            <xsl:value-of select="ancestor::SEK[1]/@uid" />
          </xsl:attribute>

          <xsl:attribute name="text">
            <xsl:for-each select="MS5/T">
              <xsl:value-of select="."/>
            </xsl:for-each>
          </xsl:attribute>
          <xsl:call-template name="GetIndexItems" />
        </item>
      </xsl:when>

    </xsl:choose>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
